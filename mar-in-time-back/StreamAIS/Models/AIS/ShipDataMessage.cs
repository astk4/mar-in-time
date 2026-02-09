namespace StreamAIS.Models.AIS
{
    public class ShipDataMessage
    {
        public ShipDataReportBody ShipStaticData { get; set; } = null!;

        public override string ToString() => ShipStaticData.ToString();
    } 
}
