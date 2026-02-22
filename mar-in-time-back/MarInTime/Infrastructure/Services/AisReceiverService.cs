using AisCommunication.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MarInTime.Infrastructure.TransportModels;
using MessagePack;
using Microsoft.AspNetCore.Connections;
using StackExchange.Redis;
using System.Diagnostics;

namespace MarInTime.Infrastructure.Services
{
    public class AisReceiverService : AisSender.AisSenderBase
    {
        private readonly StackExchange.Redis.IDatabase redisDb;
        public AisReceiverService(IConnectionMultiplexer redisMultiplexer) => redisDb = redisMultiplexer.GetDatabase();

        public override async Task<Empty> SendMessages(IAsyncStreamReader<AISResult> requestStream, ServerCallContext context)
        {
            try
            {
                await foreach (AISResult message in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    if (message.Position != null)
                    {
                        ShipPositionCheckpoint checkpoint = new ShipPositionCheckpoint()
                        {
                            MMSI = message.UserMMSI,
                            Longitude = message.Position.Longitude, 
                            Latitude = message.Position.Latitude,
                        };

                        byte[] checkpointData = MessagePackSerializer.Serialize(checkpoint);

                        await redisDb.HashSetAsync(ShipPositionCheckpoint.HashKey, message.UserMMSI, checkpointData);
                    }
                    else if (message.ShipData != null)
                    {
                        if (message.ShipData.ShipType == 0) { continue; } //no ship type available

                        ShipAppearanceCheckpointDto data = new ShipAppearanceCheckpointDto()
                        {
                            MMSI = message.UserMMSI,
                            TypeId = (int)ShipAppearanceCheckpointDto.GetTypeForNumber(message.ShipData.ShipType),
                        };
                        byte[] dataInBytes = MessagePackSerializer.Serialize(data);

                        await redisDb.HashSetAsync(ShipAppearanceCheckpointDto.HashKey, message.UserMMSI, dataInBytes);
                    }
                    else
                    {
                        Console.WriteLine($"GRPC received safety message: {message.Safety.Text}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("grpc server turned off");
            }
            catch (IOException io)
            {
                if (io.InnerException is not ConnectionAbortedException)
                {
                    throw;
                }
                Debug.WriteLine("grpc communication stream aborted");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("grpc server error " + ex.Message);
            }
            
            return new Empty();
        }
    }
}
