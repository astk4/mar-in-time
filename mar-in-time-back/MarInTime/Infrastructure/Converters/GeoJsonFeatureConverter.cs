using MarInTime.Infrastructure.TransportModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarInTime.Infrastructure.Converters
{
    public class GeoJsonFeatureConverter : JsonConverter<SpatialEntityDisplayDto>
    {
        public override SpatialEntityDisplayDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, SpatialEntityDisplayDto value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("type", "Feature");

            writer.WritePropertyName("properties");
            writer.WriteStartObject();

            writer.WriteNumber(ConvertNameIfPolicyExists(nameof(value.GID), options.PropertyNamingPolicy), value.GID);

            writer.WriteEndObject();

            writer.WritePropertyName("geometry");
            writer.WriteRawValue(value.GeometryAsString, true);

            writer.WriteEndObject();
        }

        protected static string ConvertNameIfPolicyExists(string name, JsonNamingPolicy? policy) 
        {
            if (policy == null)
            {
                return name;
            }
            return policy.ConvertName(name);
        }
    }
}
