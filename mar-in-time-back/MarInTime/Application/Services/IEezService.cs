using MarInTime.Infrastructure.TransportModels;

namespace MarInTime.Application.Services
{
    public interface IEezService
    {
        bool ZoomTierEquals(int zoom1, int zoom2);
        IAsyncEnumerable<SpatialEntityDisplayDto> GetZone(int zoom, double minLng, double minLat, double maxLng, double maxLat);
    }
}
