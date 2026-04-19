using StreamAIS.Models.Abstract;
using System.Text;

namespace StreamAIS.Models.AIS
{
    public class PositionReportBody : AbstractAisReportBody
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool PositionAccuracy { get; set; }

        public byte NavigationalStatus { get; set; }
        public sbyte RateOfTurn { get; set; }

        public double Cog { get; set; }
        public double Sog { get; set; }
        public short TrueHeading { get; set; }

        public byte TimeStamp { get; set; }

        public byte SpecialManoeuvreIndicator { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(110);

            sb.Append("Ship MMSI ").Append(UserID)
                .Append(" at ").Append(Latitude).Append(", ").Append(Longitude)
                .Append(" cog ").Append(Cog)
                .Append(" sog ").Append(Sog);

            return sb.ToString();
        }
    }
}
