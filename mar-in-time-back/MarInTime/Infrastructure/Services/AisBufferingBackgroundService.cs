using MarInTime.Application.Services;
using MarInTime.Infrastructure.TransportModels;
using MarInTime.Presentation;
using MarInTime.Presentation.ViewModels;
using MessagePack;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Diagnostics;

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
                    HashEntry[] allEntries = redisDb.HashGetAll(ShipPositionCheckpoint.HashKey);

                    ShipPositionCheckpoint[] shipMarkers 
                        = allEntries.Select(entry => MessagePackSerializer.Deserialize<ShipPositionCheckpoint>(entry.Value))
                                    .ToArray();
                    await trafficResultHubContext.Clients.All.SendAsync(AisResultHub.ReceiveShipPosKey, shipMarkers);

                    Debug.WriteLine($"Sending out {allEntries.Length} entries");
                }
            }
        }
    }
}
