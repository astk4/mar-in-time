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
        private const string ExpiryKey = "Redis:ExpiryMinutes:AIS";

        private readonly StackExchange.Redis.IDatabase redisDb;
        private readonly IConfiguration configuration;
        public AisReceiverService(IConnectionMultiplexer redisMultiplexer, IConfiguration configuration)
        {
            redisDb = redisMultiplexer.GetDatabase();
            this.configuration = configuration;
        }

        public override async Task<Empty> SendMessages(IAsyncStreamReader<AISResult> requestStream, ServerCallContext context)
        {
            try
            {
                await foreach (AISResult message in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    if (message.Safety != null)
                    {
                        Console.WriteLine($"GRPC received safety message: {message.Safety.Text}");
                        continue;
                    }

                    double minutes = configuration.GetValue<double>(ExpiryKey);
                    TimeSpan expiry = TimeSpan.FromMinutes(minutes);

                    if (message.Position != null)
                    {
                        await redisDb.HashFieldSetAndSetExpiryAsync(ShipCheckpointDto.HashKey,
                                                   $"{message.UserMMSI}:{ShipCheckpointDto.LatitudeHashField}",
                                                   message.Position.Latitude,
                                                   expiry);

                        await redisDb.HashFieldSetAndSetExpiryAsync(ShipCheckpointDto.HashKey,
                                                   $"{message.UserMMSI}:{ShipCheckpointDto.LongitudeHashField}",
                                                   message.Position.Longitude,
                                                   expiry);

                        double course = GetCourse(message.Position);

                        if (course < 360) {
                            await redisDb.HashFieldSetAndSetExpiryAsync(ShipCheckpointDto.HashKey,
                                                                   $"{message.UserMMSI}:{ShipCheckpointDto.CourseHashField}", 
                                                                   course, expiry); 
                        }
                    }
                    else //if message.ShipData != null
                    {
                        if (message.ShipData.ShipType == 0) { continue; } //no ship type available

                        int typeId = (int)ShipCheckpointDto.GetTypeForNumber(message.ShipData.ShipType);

                        await redisDb.HashFieldSetAndSetExpiryAsync(ShipCheckpointDto.HashKey,
                                                   $"{message.UserMMSI}:{ShipCheckpointDto.TypeHashField}", typeId, expiry);
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
