using StreamAIS.Models.Abstract;
using System.Text;

namespace StreamAIS.Models.AIS
{
    public class ShipDataReportBody : AbstractShipDataReportBody
    {
        public int ImoNumber { get; set; }

        public string Name { get; set; } = string.Empty;

        public AisDateTime Eta { get; set; } = null!;

        public double MaximumStaticDraught { get; set; }

        public string Destination { get; set; } = string.Empty;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(110);

            sb.Append("IMO").Append(ImoNumber).Append(' ')
                .Append(Name)
                .Append(" destination ").Append(Destination)
                .Append(" ETA ").Append(Eta);

            return sb.ToString();
        }
    }
}
