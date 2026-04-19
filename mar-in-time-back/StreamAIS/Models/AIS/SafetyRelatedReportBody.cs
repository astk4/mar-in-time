using StreamAIS.Models.Abstract;
using System.Text;

namespace StreamAIS.Models.AIS
{
    public class SafetyRelatedReportBody : AbstractAisReportBody
    {
        public string Text { get; set; } = string.Empty;
        public int? DestinationID { get; set; }
        public bool? Retransmission { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Text.Length + 20);

            sb.Append("Safety message! ").Append(Text)
              .Append(" MMSI ").Append(UserID);

            if (DestinationID.HasValue)
            {
                sb.Append(" Destination MMSI ").Append(DestinationID.Value);
            }

            return sb.ToString();
        }
    }
}
