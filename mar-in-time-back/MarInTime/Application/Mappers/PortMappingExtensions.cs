using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;

namespace MarInTime.Application.Mappers
{
    public static class PortMappingExtensions
    {
        public static PortDto ToDto(this Port port)
        {
            return new PortDto(port.CountryId + port.Id,
                               port.Name ?? string.Empty,
                               port.Country?.Name ?? string.Empty,
                               port.Routes?.Where(r => r.Name!=null).Select(r => r.Name!).ToArray() ?? Array.Empty<string>(),
                               port.Latitude,
                               port.Longitude);
        }
    }
}
