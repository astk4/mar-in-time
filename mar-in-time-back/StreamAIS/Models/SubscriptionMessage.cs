using System.Text.Json.Serialization;

namespace StreamAIS.Models
{
    public class SubscriptionMessage
    {
        public string ApiKey { get; set; } = string.Empty;

        public double[][][] BoundingBoxes { get; set; } = null!;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? FilterMessageTypes {  get; set; }
    }
}
