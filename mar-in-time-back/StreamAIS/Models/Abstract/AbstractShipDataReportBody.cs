using System.Text;

namespace StreamAIS.Models.Abstract
{
    public abstract class AbstractShipDataReportBody : AbstractAisReportBody
    {
        public class ShipDimension
        {
            public short A {  get; set; }
            public short B { get; set; }
            public short C { get; set; }
            public short D { get; set; }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder(15);
                sb.Append(A).Append("x")
                  .Append(B).Append("x")
                  .Append(C).Append("x").Append(D);
                return sb.ToString();
            }
        }

        public string CallSign { get; set; } = string.Empty;
        public ShipDimension Dimension { get; set; } = null!;
        public virtual byte Type { get; set; }
    }
}
