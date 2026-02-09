namespace StreamAIS.Models.AIS
{
    public class PositionMessage
    {
        private PositionReportBody ActualReportBody
        {
            get => PositionReport == null ? StandardClassBPositionReport! : PositionReport;
        }

        public PositionReportBody? PositionReport { get; set; }
        public PositionReportBody? StandardClassBPositionReport {  get; set; }

        public override string ToString() => ActualReportBody.ToString();
    }
}
