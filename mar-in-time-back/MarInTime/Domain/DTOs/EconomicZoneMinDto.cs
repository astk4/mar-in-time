using MarInTime.Infrastructure.Converters;
using System.Text.Json.Serialization;

namespace MarInTime.Domain.DTOs
{
    public record EconomicZoneMinDto(int GID,
                                         string Name,
                                         double CentroidLatitude,
                                         double CentroidLongitude)
    {
    }
}
