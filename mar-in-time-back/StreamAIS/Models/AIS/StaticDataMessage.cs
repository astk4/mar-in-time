namespace StreamAIS.Models.AIS
{
    public class StaticDataMessage
    {
        public StaticDataReportBody StaticDataReport { get; set; } = null!;

        public override string ToString() => StaticDataReport.ToString();
    }
}
