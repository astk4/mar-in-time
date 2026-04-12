using AisCommunication.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MarInTime.Infrastructure.TransportModels;
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

                        double course = GetCourse(message.Position);

                        if (course < 360) {
                            await redisDb.HashSetAsync(ShipCheckpointDto.HashKey,
                                                       $"{message.UserMMSI}:{ShipCheckpointDto.CourseHashField}", course); 
                        }
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

        private static double GetCourse(PositionReport positionReport)
        {
            if (positionReport.Sog < double.Epsilon || positionReport.Sog >= 102.3)
            {
                return 360;
            }

            switch (positionReport.NavigationalStatus)
            {
                case 1 or 5: //at anchor or moored
                    return 360;
                case 6: //aground
                    return positionReport.Sog > 0.5 ? positionReport.TrueHeading : 360;
                default:
                    return positionReport.Sog > 0.2? positionReport.Cog : positionReport.TrueHeading; 
            }
        }
    }
}
