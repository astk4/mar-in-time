using MarInTime.Application.Repositories;
using MarInTime.Application.Services;
using MarInTime.Infrastructure.TransportModels;

namespace MarInTime.Infrastructure.Services
{
    public class EezService : IEezService
    {
        private const string GetZoomsFuncName = "get_eez_zoom_names";
        private readonly IGisRepository gisRepository;

        public EezService(IGisRepository gisRepository)
        {
            this.gisRepository = gisRepository;
        }

        private static (int, int) ZoomNameToRange(string levelName)
        {
            string[] split = levelName.Split('_');
            int minLevel = int.Parse(split[^2]);
            int maxLevel = int.Parse(split[^1]);
            return (minLevel, maxLevel);
        }

        public IAsyncEnumerable<SpatialEntityDisplayDto> GetZone(int zoom, double minLng, double minLat, double maxLng, double maxLat)
        {
            IEnumerable<string> allRelNames = gisRepository.GetRelationNames(GetZoomsFuncName);
            string zoomRelName = allRelNames.First(n =>
            {
                (int min, int max) tierRange = ZoomNameToRange(n);
                return tierRange.min <= zoom && tierRange.max >= zoom;
            });
            return this.gisRepository.GetChunksInBounds(zoomRelName, minLng, minLat, maxLng, maxLat, zoom > 4);
        }

        public bool ZoomTierEquals(int zoom1, int zoom2)
        {
            if (zoom1 == zoom2)
            {
                return true;
            }

            IEnumerable<string> allRelNames = gisRepository.GetRelationNames(GetZoomsFuncName);
            return allRelNames.Any(n =>
            {
                (int min, int max) tierRange = ZoomNameToRange(n);

                return tierRange.min <= zoom1 && tierRange.max >= zoom1 
                    && tierRange.min <= zoom2 && tierRange.max >= zoom2;
            });
        }
    }
}
