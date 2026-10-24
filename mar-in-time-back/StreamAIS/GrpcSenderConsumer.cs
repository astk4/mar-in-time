using AisCommunication.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using StreamAIS.Models;
using StreamAIS.Models.AIS;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace StreamAIS
{
    internal class GrpcSenderConsumer : AbstractConsumer
    {
        private static readonly byte[]
            positionReportStr = Encoding.UTF8.GetBytes("PositionReport"),
            shipDataStr = Encoding.UTF8.GetBytes("ShipStaticData"),
            staticDataStr = Encoding.UTF8.GetBytes("StaticDataReport"),
            safeBroadcastStr = Encoding.UTF8.GetBytes("SafetyBroadcastMessage"),
            safeAddrStr = Encoding.UTF8.GetBytes("AddressedSafetyMessage"),
            subscriptionConfirm = Encoding.UTF8.GetBytes("SubscriptionConfirmation");

        private static readonly byte[] msgTypePropertyBytes = Encoding.UTF8.GetBytes("MessageType");

        private readonly GrpcChannel grpcChannel;
        private readonly AsyncClientStreamingCall<AISResult, Empty> grpcStreamer;

        private readonly Channel<MessageKitDto> messageChannel;

        public Action<AISResult>? OnEntryObtained;
        private bool disposedValue;

        private readonly int pingWaitMillis;

        public GrpcSenderConsumer(IConfiguration configuration,
                                  string envKey,
                                  Channel<MessageKitDto> messageChannel,
                                  int maxTypeLength)
            : base(configuration)
        {
            string targetUrl = configuration["Grpc:TargetUrl:"+envKey]!;
            int maxRetryAttempts = configuration.GetValue<int>("Grpc:MaxRetryAttempts", 10);

            this.grpcChannel = GrpcChannel.ForAddress(targetUrl, new GrpcChannelOptions
            {
                MaxRetryAttempts = maxRetryAttempts,
                ServiceConfig = new ServiceConfig() 
                {
                    MethodConfigs = 
                    {
                        new MethodConfig() 
                        {
                            Names = { MethodName.Default },
                            RetryPolicy = new RetryPolicy()
                            {
                                MaxAttempts = maxRetryAttempts,
                                InitialBackoff = TimeSpan.FromSeconds(1),
                                MaxBackoff = TimeSpan.FromSeconds(10),
                                BackoffMultiplier = 1.25,
                                RetryableStatusCodes = { StatusCode.Unavailable, StatusCode.ResourceExhausted }
                            }
                        }
                    }
                }
            });
            this.pingWaitMillis = configuration.GetValue<int>("Grpc:WaitPingMillis", 25);

            AisSender.AisSenderClient senderClient = new AisSender.AisSenderClient(grpcChannel);
            this.grpcStreamer = senderClient.SendMessages();

            this.messageChannel = messageChannel;
        }

        public override async Task ConsumingLoop()
        {
            if (this.grpcChannel.State != ConnectivityState.Ready) {
                Console.WriteLine("gRPC connection is not ready yet, waiting for backend to launch its part");
            }
            while (this.grpcChannel.State != ConnectivityState.Ready) {
                Thread.Sleep(this.pingWaitMillis);
            }

            await foreach (MessageKitDto msg in messageChannel.Reader.ReadAllAsync())
            {
                AISResult? aisRes = ProcessMessageBytes(msg.MessageBuffer, msg.BytesCount);

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
                    Console.WriteLine(grpcChannel.Target);
                }
                finally
                {
                    OnEntryObtained?.Invoke(aisRes);

                    ArrayPool<byte>.Shared.Return(msg.MessageBuffer, true);
                }
            }
        }

        private AISResult? ProcessMessageBytes(byte[] message, int bytesCount)
        {
            ReadOnlySpan<byte> currentMsgTypeBytes = GetMessageTypeName(message);

            if (currentMsgTypeBytes.SequenceEqual(shipDataStr))
            {
                AisMessageWrapper<ShipDataMessage> shipMessageFromJson
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                     AisJsonSerializationContext.Default.AisMessageWrapperShipDataMessage)!;

                AISResult shipResult = new AISResult() { ShipData = shipMessageFromJson.Message.ToProto() };
                shipMessageFromJson.Message.ShipStaticData.MapOntoProto(shipResult);
                return shipResult;
            }

            if (currentMsgTypeBytes.SequenceEqual(staticDataStr))
            {
                AisMessageWrapper<StaticDataMessage> staticMessageFromJson
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                     AisJsonSerializationContext.Default.AisMessageWrapperStaticDataMessage)!;

                AISResult staticResult = new AISResult() { ShipData = staticMessageFromJson.Message.ToProto() };
                staticMessageFromJson.Message.StaticDataReport.MapOntoProto(staticResult);
                return staticResult;
            }

            if (currentMsgTypeBytes.SequenceEqual(safeBroadcastStr) || currentMsgTypeBytes.SequenceEqual(safeAddrStr))
            {
                AisMessageWrapper<SafetyRelatedMessage> safetyMessageFromJson
                        = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                 AisJsonSerializationContext.Default.AisMessageWrapperSafetyRelatedMessage)!;

                AISResult safetyResult = new AISResult() { Safety = safetyMessageFromJson.Message.ToProto() };
                safetyMessageFromJson.Message.ActualSafetyMessage.MapOntoProto(safetyResult);
                return safetyResult;
            }

            if (currentMsgTypeBytes.EndsWith(positionReportStr)) // PositionReport, StandardClassBPositionReport, ExtendedClassBPositionReport
            {
                AisMessageWrapper<PositionMessage> positionFromJson
                            = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                         AisJsonSerializationContext.Default.AisMessageWrapperPositionMessage)!;

                AISResult positionResult = new AISResult() { Position = positionFromJson.Message.ToProto() };

                if (positionResult.Position.ClassB)
                {
                    positionFromJson.Message.StandardClassBPositionReport!.MapOntoProto(positionResult);
                }
                else
                {
                    positionFromJson.Message.PositionReport!.MapOntoProto(positionResult);
                }
                return positionResult;
            }

            if (currentMsgTypeBytes.SequenceEqual(subscriptionConfirm))
            {
                AisMessageWrapper<SubscriptionConfirmMessage> confirmFromJson
                    = JsonSerializer.Deserialize(message.AsSpan(0, bytesCount),
                                                 AisJsonSerializationContext.Default.AisMessageWrapperSubscriptionConfirmMessage)!;
                Console.WriteLine($"Compression negotiated: {confirmFromJson.Message.CompressionEnabled}");
            }

            return null;
        }

        private static ReadOnlySpan<byte> GetMessageTypeName(byte[] message)
        {
            Utf8JsonReader reader = new Utf8JsonReader(message);
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName
                    && reader.ValueSpan.SequenceEqual(msgTypePropertyBytes))
                {
                    reader.Read(); // read next token which is actual message type name
                    break;
                }
            }
            return reader.ValueSpan;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.grpcStreamer.Dispose();
                    this.grpcChannel.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
