using StreamAIS.Models.Abstract;
using System.Text;

namespace StreamAIS.Models.AIS
{
    public class StaticDataReportBody : AbstractAisReportBody
    {
        public StaticDataReportPartA? ReportA { get; set; }
        public StaticDataReportPartB? ReportB { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(110);

            if (ReportA != null)
            {
                sb.Append("Ship ").Append(ReportA.Name);
            }

            if (ReportB != null)
            {
                sb.Append(" Dimensions: ").Append(ReportB.Dimension)
                    .Append("Model ").Append(ReportB.VenderIDSerial)
                    .Append(' ').Append(ReportB.VenderIDName);
            }

            return sb.ToString();
        }
    }
}
