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
                        await redisDb.HashSetAsync(ShipCheckpointDto.HashKey,
                                                   $"{message.UserMMSI}:{ShipCheckpointDto.LatitudeHashField}", 
                                                   message.Position.Latitude);

                        await redisDb.HashSetAsync(ShipCheckpointDto.HashKey,
                                                   $"{message.UserMMSI}:{ShipCheckpointDto.LongitudeHashField}", 
                                                   message.Position.Longitude);
                    }
                    else if (message.ShipData != null)
                    {
                        if (message.ShipData.ShipType == 0) { continue; } //no ship type available

                        int typeId = (int)ShipCheckpointDto.GetTypeForNumber(message.ShipData.ShipType);

                        await redisDb.HashSetAsync(ShipCheckpointDto.HashKey,
                                                   $"{message.UserMMSI}:{ShipCheckpointDto.TypeHashField}", typeId);
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
