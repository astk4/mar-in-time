using MarInTime.Infrastructure.TransportModels;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarInTime.Infrastructure.Converters
{
    public abstract class AbstractGeoJsonConverter<T> : JsonConverter<T> where T : SpatialEntityDisplayDto
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("type", "Feature");

            writer.WritePropertyName("properties");
            writer.WriteStartObject();

            writer.WriteNumber(ConvertNameIfPolicyExists(nameof(value.GID), options.PropertyNamingPolicy), value.GID);
            WritePropertiesPart(writer, value, options.PropertyNamingPolicy);

            writer.WriteEndObject();

            writer.WritePropertyName("geometry");
            writer.WriteRawValue(value.GeometryAsString, true);

            writer.WriteEndObject();
        }

        public abstract void WritePropertiesPart(Utf8JsonWriter writer, T value, JsonNamingPolicy? policy);

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
