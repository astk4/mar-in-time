using MarInTime.Infrastructure.TransportModels;
using MarInTime.Presentation;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace MarInTime.Infrastructure.Services
{
    public class AisBufferingBackgroundService : BackgroundService
    {
        private const int frequencyMillis = 2000;

        private readonly StackExchange.Redis.IDatabase redisDb;
        private readonly IHubContext<AisResultHub> trafficResultHubContext;

        private readonly ConcurrentDictionary<string, ShipCheckpointDto> mmsiShipMap
            = new ConcurrentDictionary<string, ShipCheckpointDto>(StringComparer.OrdinalIgnoreCase);

        private static int ParseMmsiNoCulture(string str)
        {
            int result = 0;
            for (byte i = 0; i<9; i++)
            {
                result = result * 10 + (str[i] - '0');
            }
            return result;
        }

        public AisBufferingBackgroundService(IConnectionMultiplexer redisMultiplexer, IHubContext<AisResultHub> trafficResultHub)
        {
            redisDb = redisMultiplexer.GetDatabase();
            this.trafficResultHubContext = trafficResultHub;
        } 

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(frequencyMillis)))
            {
                while(!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync())
                {
                    HashEntry[] typeEntries = redisDb.HashGetAll(ShipCheckpointDto.HashKey);

                    if (typeEntries.Length == 0) { continue; }

                    string crtImoSubstring, propName;

                    foreach (HashEntry entry in typeEntries)
                    {
                        if (((string)entry.Name)!.Length < 10) { continue; }
                        
                        crtImoSubstring = ((string)entry.Name)!.Substring(0, 9);
                        
                        ShipCheckpointDto ship 
                            = mmsiShipMap.GetOrAdd(crtImoSubstring, 
                                             new ShipCheckpointDto() { MMSI = ParseMmsiNoCulture(crtImoSubstring) });
                        
                        propName = ((string)entry.Name)!.Substring(10);

                        switch (propName)
                        {
                            case ShipCheckpointDto.LatitudeHashField:

                                ship.Latitude = (double)entry.Value;
                                break;
                            case ShipCheckpointDto.LongitudeHashField:

                                ship.Longitude = (double)entry.Value;
                                break;
                            case ShipCheckpointDto.TypeHashField:

                                ship.TypeId = (int)entry.Value;
                                break;
                            case ShipCheckpointDto.CourseHashField:
                                
                                ship.Course = (double)entry.Value;
                                break;
                            default:
                                break;
                        }
                    }

                    await trafficResultHubContext.Clients.All.SendAsync(AisResultHub.ReceiveShipCheckpointsKey, 
                                                                        mmsiShipMap.Values, stoppingToken);
                }
            }
        }
    }
}
