using MarInTime.Infrastructure.TransportModels;

namespace MarInTime.Application.Services
{
    public interface IGisService<TDto>
    {
        bool ZoomTierEquals(int zoom1, int zoom2);
        IAsyncEnumerable<SpatialEntityDisplayDto> GetZone(int zoom, double minLng, double minLat, double maxLng, double maxLat);
        Task<TDto?> TryGetAtCoordinates(double lng, double lat);
    }
}
