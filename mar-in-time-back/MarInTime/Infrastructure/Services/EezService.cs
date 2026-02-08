using MarInTime.Application.Mappers;
using MarInTime.Application.Repositories;
using MarInTime.Application.Services;
using MarInTime.Domain;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using MarInTime.Infrastructure.TransportModels;

namespace MarInTime.Infrastructure.Services
{
    public class EezService : IGisService<EconomicZoneDto>
    {
        private const string GetZoomsFuncName = "get_eez_zoom_names";
        private readonly IGisStreamRepository gisRepository;
        private readonly ISpatialRepository<ExclusiveEconomicZone> eezSpatialRepository;

        public EezService(IGisStreamRepository gisRepository, ISpatialRepository<ExclusiveEconomicZone> eezSpatialRepository)
        {
            this.gisRepository = gisRepository;
            this.eezSpatialRepository = eezSpatialRepository;
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

        public async Task<EconomicZoneDto?> TryGetAtCoordinates(double lng, double lat)
        {
            var iReadOnlyList = await eezSpatialRepository.GetZonesAt(lng, lat);
            if (!iReadOnlyList.Any())
            {
                return null;
            }

            return iReadOnlyList[0].ToDto();
        }
    }
}
