using MarInTime.Infrastructure.Converters;
using System.Text.Json.Serialization;

namespace MarInTime.Infrastructure.TransportModels
{
    [JsonConverter(typeof(EconomicZoneGeoJsonConverter))]
    public record EconomicZoneDisplayDto(int GID, 
                                         string Name,
                                         string GeometryAsString,
                                         double CentroidLatitude, 
                                         double CentroidLongitude)
        : SpatialEntityDisplayDto(GID, Name, GeometryAsString)
    {
    }
}
