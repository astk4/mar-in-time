using AisCommunication.Shared;
using StreamAIS.Models.Abstract;
using StreamAIS.Models.AIS;

namespace StreamAIS
{
    internal static class ProtoMappingExtensions
    {
        static private void MapAbstractShipOnProto(this AbstractShipDataReportBody abstractInput, ShipDataReport result)
        {
            result.ShipType = abstractInput.Type;

            result.DimA = abstractInput.Dimension.A;
            result.DimB = abstractInput.Dimension.B;
            result.DimC = abstractInput.Dimension.C;
            result.DimD = abstractInput.Dimension.D;

            result.CallSign = abstractInput.CallSign.Trim();
        }

        public static void MapOntoProto(this AbstractAisReportBody abstractAis, AISResult result)
        {
            result.UserMMSI = abstractAis.UserID;
            result.Repeated = abstractAis.RepeatIndicator > 0;
        }

        static public PositionReport ToProto(this PositionMessage message)
        {
            bool _classB = message.StandardClassBPositionReport != null;
            PositionReportBody reportBody = (_classB ? message.StandardClassBPositionReport : message.PositionReport)!;

            return new PositionReport()
            {
                Latitude = reportBody.Latitude,
                Longitude = reportBody.Longitude,
                PositionAccuracy = reportBody.PositionAccuracy,

                NavigationalStatus = reportBody.NavigationalStatus,
                RateOfTurn = reportBody.RateOfTurn,

                Cog = reportBody.Cog,
                Sog = reportBody.Sog,
                TrueHeading = reportBody.TrueHeading,
                TimeStopSeconds = reportBody.TimeStamp,
                SpecialManeuver = reportBody.SpecialManoeuvreIndicator,

                ClassB = _classB,
            };
        }

        static public ShipDataReport ToProto(this ShipDataMessage message)
        {
            ShipDataReportBody reportBody = message.ShipStaticData;

            var outputReport = new ShipDataReport()
            {
                IMONumber = reportBody.ImoNumber,

                Name = reportBody.Name.Trim(),

                EtaMonth = reportBody.Eta.Month,
                EtaDay = reportBody.Eta.Day,
                EtaHour = reportBody.Eta.Hour,
                EtaMinute = reportBody.Eta.Minute,

                MaxStaticDraught = reportBody.MaximumStaticDraught,
                Destination = reportBody.Destination.Trim()
            };

            reportBody.MapAbstractShipOnProto(outputReport);
            return outputReport;
        }

        static public ShipDataReport ToProto(this StaticDataMessage message)
        {
            StaticDataReportBody reportBody = message.StaticDataReport;

            var outputReport = new ShipDataReport()
            {
                Name = reportBody.ReportA?.Name.Trim(),
            };

            if (reportBody.ReportB != null)
            {
                reportBody.ReportB.MapAbstractShipOnProto(outputReport);

                outputReport.VendorIdModel = reportBody.ReportB.VenderIDModel;
                outputReport.VendorIdSerial = reportBody.ReportB.VenderIDSerial;
                outputReport.VendorIdName = reportBody.ReportB.VenderIDName;
            }
            return outputReport;
        }

        public static SafetyMessage ToProto(this SafetyRelatedMessage inputMessage)
        {
            return new SafetyMessage()
            {
                Text = inputMessage.ActualSafetyMessage.Text.Trim(),
                DestinationMMSI = inputMessage.ActualSafetyMessage.DestinationID.GetValueOrDefault(),
                Retransmission = inputMessage.ActualSafetyMessage.Retransmission.GetValueOrDefault(),
            };
        }
    }
}
