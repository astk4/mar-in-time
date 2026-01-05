using MarInTime.Infrastructure.Converters;
using System.Text.Json.Serialization;

namespace MarInTime.Infrastructure.TransportModels
{
    [JsonConverter(typeof(GeoJsonFeatureConverter))]
    public record SpatialEntityDisplayDto(int GID, string GeometryAsString);
}
