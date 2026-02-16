using System.Text.Json.Serialization;

namespace StreamAIS.Models.AIS
{
    public class SafetyRelatedMessage
    {
        [JsonIgnore]
        public SafetyRelatedReportBody ActualSafetyMessage
        {
            get => SafetyBroadcastMessage == null ? AddressedSafetyMessage! : SafetyBroadcastMessage!;
        }
        public SafetyRelatedReportBody? SafetyBroadcastMessage {  get; set; }
        public SafetyRelatedReportBody? AddressedSafetyMessage { get; set; }

        public override string ToString() => ActualSafetyMessage.ToString();
    }
}
