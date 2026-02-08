using System.Net.WebSockets;
using System.Text;
using StreamAIS.Models;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace StreamAIS
{
    internal static class Program
    {
        private const int BufferSize = 8192;
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
                   crtBuffer = new byte[BufferSize];
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

                Console.WriteLine(Encoding.UTF8.GetString(prevMsgBuffer));
                Console.WriteLine();
            }
        }
    }
}
