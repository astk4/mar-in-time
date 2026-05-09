using System.Text;

namespace StreamAIS.Models.AIS
{
    public class AisDateTime
    {
        public byte Minute { get; set; }
        public byte Hour { get; set; }
        public byte Day { get; set; }
        public byte Month { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(11);
            sb.Append(Day).Append('.').Append(Month).Append(' ')
                .Append(Hour).Append(':').Append(Minute);
            return sb.ToString();
        }
    }
}
