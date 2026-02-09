using Microsoft.Extensions.Configuration;
using StreamAIS.Models;
using StreamAIS.Models.AIS;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

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

        static void Main(string[] args)
        {
            string url = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"]!;

            using (ClientWebSocket cws = new ClientWebSocket())
            {
                cws.ConnectAsync(new Uri(url), CancellationToken.None).Wait();
                Console.WriteLine("connected");

                byte[] subscriptionBytes = GetSubscriptionMessageBytes();

                cws.SendAsync(subscriptionBytes, WebSocketMessageType.Text, true, CancellationToken.None).Wait();

                WebSocketCloseStatus futureCloseStatus = WebSocketCloseStatus.NormalClosure;
                string? explMessage = null;
                try
                {
                    WebSocketConsume(cws).Wait();
                }
                catch (Exception ex)
                {
                    futureCloseStatus = WebSocketCloseStatus.InternalServerError;
                    explMessage = ex.GetType().Name;
                    Console.WriteLine($"Closing because of exception!!! {ex.Message}");
                }
                cws.CloseAsync(futureCloseStatus, explMessage, CancellationToken.None).Wait();
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

        static private async Task WebSocketConsume(ClientWebSocket cws)
        {
            byte[] prevMsgBuffer = new byte[BufferSize],
                   crtBuffer = new byte[BufferSize],
                   messageTypeBuffer = new byte[maxTypeLength];
            int prevCount = 0;
            while (true)
            {
                WebSocketReceiveResult result = await cws.ReceiveAsync(crtBuffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                if (!result.EndOfMessage)
                {
                    Array.Copy(crtBuffer, 0, prevMsgBuffer, prevCount, result.Count);
                    prevCount += result.Count;
                    
                    continue;
                }

                Array.Fill<byte>(prevMsgBuffer, 0, result.Count, BufferSize - result.Count);
                Array.Copy(crtBuffer, prevMsgBuffer, result.Count);

                prevCount = result.Count;

                Array.Fill<byte>(messageTypeBuffer, 0, 0, maxTypeLength);

                ProcessMessageBytes(prevMsgBuffer, result.Count, ref messageTypeBuffer);
            }
        }

        private static void ProcessMessageBytes(byte[] message, int bytesCount, ref byte[] bufferForType)
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