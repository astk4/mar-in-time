using MarInTime.Application.Repositories;
using MarInTime.Application.Services;
using MarInTime.Infrastructure.TransportModels;

namespace MarInTime.Infrastructure.Services
{
    public class EezService : IEezService
    {
        private readonly IGisRepository gisRepository;

        private static IEnumerable<string>? zoomLevelNames;

        public EezService(IGisRepository gisRepository)
        {
            this.gisRepository = gisRepository;
        }

        private static bool ZoomLevelMatches(string levelName, int zoom)
        {
            string[] split = levelName.Split('_');
            int minLevel = int.Parse(split[^2]);
            if(zoom < minLevel)
            {
                return false;
            }
            int maxLevel = int.Parse(split[^1]);
            return zoom <= maxLevel;
        }

        public IAsyncEnumerable<SpatialEntityDisplayDto> GetZone(int zoom, double minLng, double minLat, double maxLng, double maxLat)
        {
            string zoomRelName = zoomLevelNames!.First(n => ZoomLevelMatches(n, zoom));
            return this.gisRepository.GetChunksInBounds(zoomRelName, minLng, minLat, maxLng, maxLat);
        }

        public void InitZoomTableNames()
        {
            if (zoomLevelNames != null)
            {
                return;
            }
            zoomLevelNames = this.gisRepository.GetRelationNames("get_eez_zoom_names");
        }
    }
}
