using AisCommunication.Shared;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using StreamAIS.Models;
using StreamAIS.Models.AIS;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Net.Security;
using ClickHouse.Driver;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace StreamAIS
{
    internal static class Program
    {
        private const int BufferSize = 8192;
        private const string positionReportStr = "PositionReport",
                             shipDataStr = "ShipStaticData",
                             staticDataStr = "StaticDataReport",
                             safeBroadcastStr = "SafetyBroadcastMessage",
                             safeAddrStr = "AddressedSafetyMessage";

        private static readonly int offsetBeforeMsgType = "{\"Message\":{\"".Length;
        private static readonly byte msgTypeEndByte = Encoding.UTF8.GetBytes("\"")[0];

        private static int maxTypeLength = 64;
        private static string currentMsgType = string.Empty;

        private static readonly string[] historyColumns = new string[]
        {
            "SessionId", "SessionStartTime", "SessionElapsedMillis",
            "MMSI", "RepeatIndicator", "AisMessageType",
            "Position_Lat", "Position_Long", "Position_Accuracy",
            "Position_NavigationStatus", "Position_RateOfTurn",
            "Position_CourseOverGround", "Position_SpeedOverGround",
            "Position_TrueHeading", "Position_Timestamp", 
            "Position_SpecialManeuverIndicator", "Ship_Name", "Ship_TypeId",
            "Ship_Dimension_A", "Ship_Dimension_B",
            "Ship_Dimension_C", "Ship_Dimension_D",
            "Ship_CallSign", "Ship_IMO", "Ship_ETA_Min",
            "Ship_ETA_Hour", "Ship_ETA_Day", "Ship_ETA_Month",
            "Ship_MaxStaticDraught", "Ship_DestinationName",
            "Ship_VendorIdName", "Ship_VendorIdSerial", "Ship_VendorIdModel",
            "Safety_Text", "Safety_DestinationID", "Safety_Retransmission"
        };

        static async Task Main(string[] args)
        {
            IConfigurationRoot confRoot = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json")
                                    .AddJsonFile("secrets.json")
                                    .Build();

            string aisUrl = confRoot["AIS:ApiUrl"]!;
            string targetUrl = confRoot["Grpc:TargetUrl"]!;

            var producerConsumerChannel = Channel.CreateBounded<MessageKitDto>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = true
            });

            int count = 0;

            ClientWebSocket cws = new ClientWebSocket();
            cws.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslErr) =>
            {
                if (sslErr == SslPolicyErrors.None) { return true; }

                if (chain != null && sslErr == SslPolicyErrors.RemoteCertificateChainErrors
                    && chain.ChainStatus.All(e => e.Status == X509ChainStatusFlags.NotTimeValid))
                {
                    Debug.WriteLine("ssl expired but i still must connect");
                    return true;
                }

                return false;
            };
            await cws.ConnectAsync(new Uri(aisUrl), CancellationToken.None);
            Console.WriteLine("connected");

            byte[] subscriptionBytes = GetSubscriptionMessageBytes(confRoot);

            await cws.SendAsync(subscriptionBytes, WebSocketMessageType.Text, true, CancellationToken.None);

            WebSocketCloseStatus futureCloseStatus = WebSocketCloseStatus.NormalClosure;
            string? explMessage = null;

            GrpcChannel grpcChannel = GrpcChannel.ForAddress(targetUrl);
            AisSender.AisSenderClient senderClient = new AisSender.AisSenderClient(grpcChannel);
            
            AsyncClientStreamingCall<AISResult, Empty> streamingCall = senderClient.SendMessages();

            double historyIntervalSec = confRoot.GetValue<double>("Clickhouse:BatchIntervalSec");
            int minBatchSize = confRoot.GetValue<int>("Clickhouse:MinBatchSize");
            int[] historyQueueCtr = new int[1];

            ClickHouseClient chClient = new ClickHouseClient(confRoot["Clickhouse:ConnectionString"]!);
            object sesId = await chClient.ExecuteScalarAsync("select * from AIS_SessionsAmount");

            ConcurrentQueue<object[]> wrappersForHistory = new ConcurrentQueue<object[]>();

            Task grpcConsumerTask = Task.Run(() => GrpcConsumingLoop(producerConsumerChannel, 
                                                                     streamingCall,
                                                                     wrappersForHistory, 
                                                                     historyQueueCtr,
                                                                     sesId == null? 0 : (uint)sesId + 1));
            Task historyConsumerTask = Task.Run(() => HistoryConsumingLoop(wrappersForHistory,
                                                                           chClient,
                                                                           TimeSpan.FromSeconds(historyIntervalSec), 
                                                                           historyQueueCtr, 
                                                                           minBatchSize,
                                                                           historyColumns));
            try
            {
                while (cws.State == WebSocketState.Open)
                {
                    byte[] prevMsgBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                    byte[] crtBuffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                    byte[] messageTypeBuffer = ArrayPool<byte>.Shared.Rent(maxTypeLength);

                    WebSocketReceiveResult result = await cws.ReceiveAsync(crtBuffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        ArrayPool<byte>.Shared.Return(prevMsgBuffer);
                        ArrayPool<byte>.Shared.Return(crtBuffer);
                        ArrayPool<byte>.Shared.Return(messageTypeBuffer);
                        break;
                    }

                    if (!result.EndOfMessage)
                    {
                        Array.Copy(crtBuffer, 0, prevMsgBuffer, count, result.Count);
                        count += result.Count;
                        continue;
                    }

                    Array.Fill<byte>(prevMsgBuffer, 0, result.Count, BufferSize - result.Count);
                    Array.Copy(crtBuffer, prevMsgBuffer, result.Count);
                    count = result.Count;

                    await producerConsumerChannel.Writer.WriteAsync(new MessageKitDto(prevMsgBuffer, messageTypeBuffer, count));

                    ArrayPool<byte>.Shared.Return(crtBuffer);
                }
            }
            catch (Exception ex)
            {
                futureCloseStatus = WebSocketCloseStatus.InternalServerError;
                explMessage = ex.GetType().Name;
                Console.WriteLine($"Closing because of exception!!! {ex.Message}");
            }
            finally
            {
                if (cws.State != WebSocketState.Aborted)
                {
                    await cws.CloseAsync(futureCloseStatus, explMessage, CancellationToken.None);
                }
                producerConsumerChannel.Writer.Complete();
            }

            await grpcConsumerTask;
            await historyConsumerTask;

            cws.Dispose();
            streamingCall.Dispose();
            grpcChannel.Dispose();

            chClient.Dispose();
        }

        static private byte[] GetSubscriptionMessageBytes(IConfigurationRoot confRoot)
        {                                              
            string apiKey = confRoot["AIS:ApiKey"]!;

            double minLat = Convert.ToDouble(confRoot["BoundingBox:MinLat"]),
                   maxLat = Convert.ToDouble(confRoot["BoundingBox:MaxLat"]),
                   minLng = Convert.ToDouble(confRoot["BoundingBox:MinLng"]),
                   maxLng = Convert.ToDouble(confRoot["BoundingBox:MaxLng"]);

            SubscriptionMessage messageToSend = new SubscriptionMessage()
            {
                ApiKey = apiKey,
                BoundingBoxes = new double[][][]
                {
                    new double[][]
                    {
                        new double[] { minLat, minLng },
                        new double[] { maxLat, maxLng }
                    }
                }
            };

            string[]? messageTypesFilter = confRoot.GetSection("MessageTypes").Get<string[]>();
            if (messageTypesFilter != null && messageTypesFilter.Length > 0)
            {
                maxTypeLength = messageTypesFilter.Max(s => s.Length);
                messageToSend.FilterMessageTypes = messageTypesFilter;
            }

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = false
            };
            string messageJson = JsonSerializer.Serialize(messageToSend, options);

            return Encoding.UTF8.GetBytes(messageJson);
        }

        private static async Task GrpcConsumingLoop(Channel<MessageKitDto> messageChannel, 
                                                    AsyncClientStreamingCall<AISResult, Empty> grpcStreamer,
                                                    ConcurrentQueue<object[]> queueForHistory,
                                                    int[] counterContainer,
                                                    uint sessionId)
        {
            DateTime sessionStart = DateTime.UtcNow;

            await foreach (MessageKitDto msg in messageChannel.Reader.ReadAllAsync())
            {
                AISResult? aisRes = ProcessMessageBytes(msg.MessageBuffer, msg.TypeBuffer, msg.BytesCount);
                
                if (aisRes == null) { continue; }

                try
                {
                    await grpcStreamer.RequestStream.WriteAsync(aisRes);

                    Console.WriteLine("Successful GRPC sent ");
                    if (aisRes.Position != null)
                    {
                        Console.WriteLine($"position {aisRes.Position.Latitude}, {aisRes.Position.Longitude}");
                    }
                    else if (aisRes.ShipData != null)
                    {
                        Console.WriteLine($"some data for ship IMO{aisRes.ShipData.IMONumber} {aisRes.ShipData.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"safety message: {aisRes.Safety.Text}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Consumer iteration exception: " + ex.Message);
                }
                finally
                {
                    object[] entry = ConvertAisToObject(aisRes);
                    entry[0] = sessionId;
                    entry[1] = sessionStart;
                    entry[2] = (long)((DateTime.UtcNow - sessionStart).TotalMilliseconds);

                    queueForHistory.Enqueue(entry);
                    Interlocked.Increment(ref counterContainer[0]);
                    
                    ArrayPool<byte>.Shared.Return(msg.MessageBuffer, true);
                    ArrayPool<byte>.Shared.Return(msg.TypeBuffer, true);
                }
            }
        }

        private static async Task HistoryConsumingLoop(ConcurrentQueue<object[]> queue,
                                                       ClickHouseClient chClient,
                                                       TimeSpan timerInterval, 
                                                       int[] counterContainer,
                                                       int minBatchSize,
                                                       string[] columns)
        {
            using (PeriodicTimer timer = new PeriodicTimer(timerInterval))
            {
                while(await timer.WaitForNextTickAsync())
                {
                    while (counterContainer[0] < minBatchSize) { /* wait to fill... */ }

                    await chClient.InsertBinaryAsync("AIS_History", columns, queue);

                    queue.Clear();
                    Interlocked.Add(ref counterContainer[0], counterContainer[0] * -1);
                }
            }
        }

        private static AISResult? ProcessMessageBytes(byte[] message, byte[] bufferForType, int bytesCount)
        {
            int crtMsgTypeLength = 0;

            for (int i = 0; i <= maxTypeLength; i++)
            {
                if (message[i+offsetBeforeMsgType] == msgTypeEndByte)
                {
                    crtMsgTypeLength = i;
                    break;
                }
            }

            Array.Copy(message, offsetBeforeMsgType, bufferForType, 0, crtMsgTypeLength);
            currentMsgType = Encoding.UTF8.GetString(bufferForType).TrimEnd('\0');

            switch (currentMsgType)
            {
                case shipDataStr:
                    AisMessageWrapper<ShipDataMessage> shipMessageFromJson 
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                     AisJsonSerializationContext.Default.AisMessageWrapperShipDataMessage)!;
                    
                    AISResult shipResult = new AISResult() { ShipData = shipMessageFromJson.Message.ToProto() };
                    shipMessageFromJson.Message.ShipStaticData.MapOntoProto(shipResult);
                    return shipResult;
                    //break;
                case staticDataStr:
                    AisMessageWrapper<StaticDataMessage> staticMessageFromJson 
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                     AisJsonSerializationContext.Default.AisMessageWrapperStaticDataMessage)!;

                    AISResult staticResult = new AISResult() { ShipData = staticMessageFromJson.Message.ToProto() };
                    staticMessageFromJson.Message.StaticDataReport.MapOntoProto(staticResult);
                    return staticResult;
                    //break;
                case safeBroadcastStr or safeAddrStr:
                    AisMessageWrapper<SafetyRelatedMessage> safetyMessageFromJson
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                 AisJsonSerializationContext.Default.AisMessageWrapperSafetyRelatedMessage)!;

                    AISResult safetyResult = new AISResult() { Safety = safetyMessageFromJson.Message.ToProto() };
                    safetyMessageFromJson.Message.ActualSafetyMessage.MapOntoProto(safetyResult);
                    return safetyResult;
                    //break;
                default:
                    if (currentMsgType.Substring(currentMsgType.Length - positionReportStr.Length, positionReportStr.Length) == positionReportStr)
                    {
                        AisMessageWrapper<PositionMessage> positionFromJson 
                            = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                         AisJsonSerializationContext.Default.AisMessageWrapperPositionMessage)!;

                        AISResult positionResult = new AISResult() { Position = positionFromJson.Message.ToProto() };

                        if (positionResult.Position.ClassB)
                        {
                            positionFromJson.Message.StandardClassBPositionReport!.MapOntoProto(positionResult);
                        }
                        else {
                            positionFromJson.Message.PositionReport!.MapOntoProto(positionResult);
                        }
                        return positionResult;
                    }
                    break;
            }
            return null;
        }

        private static object[] ConvertAisToObject(AISResult aismsg)
        {
            object[] result = new object[36];

            result[3] = aismsg.UserMMSI;
            result[4] = aismsg.Repeated;

            if (aismsg.Position != null)
            {
                result[5] = 1;

                result[6] = aismsg.Position.Latitude;
                result[7] = aismsg.Position.Longitude;
                result[8] = aismsg.Position.PositionAccuracy;
                result[9] = aismsg.Position.NavigationalStatus;
                result[10] = aismsg.Position.RateOfTurn;
                result[11] = aismsg.Position.Cog;
                result[12] = aismsg.Position.Sog;
                result[13] = aismsg.Position.TrueHeading;
                result[14] = aismsg.Position.TimeStopSeconds;
                result[15] = aismsg.Position.SpecialManeuver;
            }
            else if (aismsg.ShipData != null)
            {
                result[5] = 5;

                result[16] = aismsg.ShipData.Name;
                result[17] = aismsg.ShipData.ShipType;
                result[18] = aismsg.ShipData.DimA;
                result[19] = aismsg.ShipData.DimB;
                result[20] = aismsg.ShipData.DimC;
                result[21] = aismsg.ShipData.DimD;

                result[22] = aismsg.ShipData.CallSign;
                result[23] = aismsg.ShipData.IMONumber;

                result[24] = aismsg.ShipData.EtaMinute;
                result[25] = aismsg.ShipData.EtaHour;
                result[26] = aismsg.ShipData.EtaDay;
                result[27] = aismsg.ShipData.EtaMonth;

                result[28] = aismsg.ShipData.MaxStaticDraught;
                result[29] = aismsg.ShipData.Destination;

                result[30] = aismsg.ShipData.VendorIdName;
                result[31] = aismsg.ShipData.VendorIdSerial;
                result[32] = aismsg.ShipData.VendorIdModel;
            }
            else //if safety != null
            {
                result[5] = 12;

                result[33] = aismsg.Safety.Text;
                result[34] = aismsg.Safety.DestinationMMSI;
                result[35] = aismsg.Safety.Retransmission;
            }

            return result;
        }
    }
}