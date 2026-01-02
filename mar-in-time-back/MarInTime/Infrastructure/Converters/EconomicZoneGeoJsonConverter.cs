using MarInTime.Infrastructure.TransportModels;
using System.Text.Json;

namespace MarInTime.Infrastructure.Converters
{
    public class EconomicZoneGeoJsonConverter : AbstractGeoJsonConverter<EconomicZoneDisplayDto>
    {
        public override void WritePropertiesPart(Utf8JsonWriter writer, EconomicZoneDisplayDto value, JsonNamingPolicy? policy)
        {
            writer.WriteString(ConvertNameIfPolicyExists(nameof(value.Name), policy), value.Name);
            writer.WriteNumber(ConvertNameIfPolicyExists(nameof(value.CentroidLatitude), policy), value.CentroidLatitude);
            writer.WriteNumber(ConvertNameIfPolicyExists(nameof(value.CentroidLongitude), policy), value.CentroidLongitude);
        }
    }
}
