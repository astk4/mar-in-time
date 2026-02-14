using Microsoft.Extensions.Configuration;
using StreamAIS.Models;
using StreamAIS.Models.AIS;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

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

        static async Task Main(string[] args)
        {
            string url = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"]!;
            
            var producerConsumerChannel = Channel.CreateBounded<MessageKitDto>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = true
            });

            int count = 0;
            
            using (ClientWebSocket cws = new ClientWebSocket())
            {
                await cws.ConnectAsync(new Uri(url), CancellationToken.None);
                Console.WriteLine("connected");

                byte[] subscriptionBytes = GetSubscriptionMessageBytes();

                await cws.SendAsync(subscriptionBytes, WebSocketMessageType.Text, true, CancellationToken.None);

                WebSocketCloseStatus futureCloseStatus = WebSocketCloseStatus.NormalClosure;
                string? explMessage = null;
                
                Task consumerTask = Task.Run(() => StartConsumingLoop(producerConsumerChannel));
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

                await consumerTask;          
            }
        }

        static private byte[] GetSubscriptionMessageBytes()
        {
            IConfigurationRoot confRoot = new ConfigurationBuilder()
                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("ais_config.json")
                                                .Build();
                                                
            string apiKey = System.Configuration.ConfigurationManager.AppSettings["ApiKey"]!;

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

        private static async Task StartConsumingLoop(Channel<MessageKitDto> messageChannel)
        {
            await foreach (MessageKitDto msg in messageChannel.Reader.ReadAllAsync())
            {
                try
                {
                    ProcessMessageBytes(msg.MessageBuffer, msg.TypeBuffer, msg.BytesCount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Consumer iteration exception: " + ex.Message);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(msg.MessageBuffer, true);
                    ArrayPool<byte>.Shared.Return(msg.TypeBuffer, true);
                }
            }
        }

        private static void ProcessMessageBytes(byte[] message, byte[] bufferForType, int bytesCount)
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
                    AisMessageWrapper<ShipDataMessage> shipMessage 
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                     AisJsonSerializationContext.Default.AisMessageWrapperShipDataMessage)!;
                    Console.WriteLine(shipMessage.Message);
                    break;
                case staticDataStr:
                    AisMessageWrapper<StaticDataMessage> staticMessage 
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                     AisJsonSerializationContext.Default.AisMessageWrapperStaticDataMessage)!;
                    Console.WriteLine(staticMessage.Message);
                    break;
                case safeBroadcastStr or safeAddrStr:
                    AisMessageWrapper<SafetyRelatedMessage> broadcast
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                 AisJsonSerializationContext.Default.AisMessageWrapperSafetyRelatedMessage)!;
                    Console.WriteLine(broadcast.Message);
                    break;
                default:
                    if (currentMsgType.Substring(currentMsgType.Length - positionReportStr.Length, positionReportStr.Length) == positionReportStr)
                    {
                        AisMessageWrapper<PositionMessage> reportMessage 
                            = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                         AisJsonSerializationContext.Default.AisMessageWrapperPositionMessage)!;
                        Console.WriteLine(reportMessage.Message);
                        break;
                    }
                    break;
            }
        }
    }
}