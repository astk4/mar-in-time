using StreamAIS.Models.Abstract;
using System.Text.Json.Serialization;

namespace StreamAIS.Models.AIS
{
    public class StaticDataReportPartB : AbstractShipDataReportBody
    {
        [JsonPropertyName("ShipType")]
        public override byte Type { get => base.Type; set => base.Type = value; }

        public byte VenderIDModel { get; set; }
        public int VenderIDSerial { get; set; }
        public string VenderIDName { get; set; } = string.Empty;
    }
}
