using MarInTime.Infrastructure.TransportModels;

namespace MarInTime.Application.Services
{
    public interface IEezService
    {
        void InitZoomTableNames();
        IAsyncEnumerable<SpatialEntityDisplayDto> GetZone(int zoom, double minLng, double minLat, double maxLng, double maxLat);
    }
}
