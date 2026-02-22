using MarInTime.Infrastructure.TransportModels;
using MarInTime.Presentation;
using MessagePack;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace MarInTime.Infrastructure.Services
{
    public class AisBufferingBackgroundService : BackgroundService
    {
        private const int freqencyMillis = 2000;

        private readonly StackExchange.Redis.IDatabase redisDb;
        private readonly IHubContext<AisResultHub> trafficResultHubContext;

        public AisBufferingBackgroundService(IConnectionMultiplexer redisMultiplexer, IHubContext<AisResultHub> trafficResultHub)
        {
            redisDb = redisMultiplexer.GetDatabase();
            this.trafficResultHubContext = trafficResultHub;
        } 

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(freqencyMillis)))
            {
                while(!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync())
                {
                    HashEntry[] typeEntries = redisDb.HashGetAll(ShipAppearanceCheckpointDto.HashKey);
                    
                    ShipAppearanceCheckpointDto[] colorsHere
                        = typeEntries.Select(entry => MessagePackSerializer.Deserialize<ShipAppearanceCheckpointDto>(entry.Value))
                                     .ToArray();
                    await trafficResultHubContext.Clients.All.SendAsync(AisResultHub.ReceiveShipDataKey, colorsHere, stoppingToken);


                    HashEntry[] posEntries = redisDb.HashGetAll(ShipPositionCheckpoint.HashKey);

                    ShipPositionCheckpoint[] shipMarkers
                        = posEntries.Select(entry => MessagePackSerializer.Deserialize<ShipPositionCheckpoint>(entry.Value))
                                    .ToArray();

                    await trafficResultHubContext.Clients.All.SendAsync(AisResultHub.ReceiveShipPosKey, shipMarkers, stoppingToken);
                }
            }
        }
    }
}
