using Microsoft.Extensions.Configuration;
using StreamAIS.Models;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace StreamAIS
{
    internal static class Program
    {
        private const int BufferSize = 8192;
        
        private static int maxTypeLength = 64;

        static async Task Main(string[] args)
        {
            IConfigurationRoot confRoot = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json")
                                    .AddJsonFile("secrets.json")
                                    .Build();

            string aisUrl = confRoot["AIS:ApiUrl"]!;

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

            GrpcSenderConsumer grpcConsumer = new GrpcSenderConsumer(confRoot, producerConsumerChannel, maxTypeLength);
            ClickHouseConsumer chConsumer = new ClickHouseConsumer(confRoot);

            grpcConsumer.OnEntryObtained += chConsumer.CollectEntry;
            
            Task grpcTask = Task.Run(grpcConsumer.ConsumingLoop);
            Task clickhouseTask = Task.Run(chConsumer.ConsumingLoop);

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

            await grpcTask;
            await clickhouseTask;

            cws.Dispose();

            chConsumer.Dispose();
            grpcConsumer.Dispose();
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
    }
}