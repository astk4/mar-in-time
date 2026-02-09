using StreamAIS.Models.AIS;
using System.Text.Json.Serialization;

namespace StreamAIS.Models.Abstract
{
    //[JsonDerivedType(typeof(SafetyRelatedReportBody), "SafetyBroadcastMessage")]
    public abstract class AbstractAisReportBody
    {
        public int UserID { get; set; }
        public byte RepeatIndicator { get; set; }
    }
}
