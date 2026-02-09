using StreamAIS.Models.AIS;
using System.Text.Json.Serialization;

namespace StreamAIS.Models
{
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(AisMessageWrapper<PositionMessage>))]
    [JsonSerializable(typeof(AisMessageWrapper<ShipDataMessage>))]
    [JsonSerializable(typeof(AisMessageWrapper<StaticDataMessage>))]
    [JsonSerializable(typeof(AisMessageWrapper<SafetyRelatedMessage>))]
    public partial class AisJsonSerializationContext : JsonSerializerContext
    {
    }
}
