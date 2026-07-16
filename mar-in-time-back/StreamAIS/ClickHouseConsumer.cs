using AisCommunication.Shared;
using ClickHouse.Driver;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace StreamAIS
{
    internal class ClickHouseConsumer : AbstractConsumer, IDisposable
    {
        private static readonly string[] historyColumns = new string[]
        {
            "SessionId", "SessionStartTime", "SessionElapsedMillis",
            "MMSI", "RepeatIndicator", "AisMessageType",
            "Position_Lat", "Position_Long", "Position_Accuracy",
            "Position_NavigationStatus", "Position_RateOfTurn",
            "Position_CourseOverGround", "Position_SpeedOverGround",
            "Position_TrueHeading", "Position_Timestamp",
            "Position_SpecialManeuverIndicator", "Ship_Name", "Ship_TypeId",
            "Ship_Dimension_A", "Ship_Dimension_B",
            "Ship_Dimension_C", "Ship_Dimension_D",
            "Ship_CallSign", "Ship_IMO", "Ship_ETA_Min",
            "Ship_ETA_Hour", "Ship_ETA_Day", "Ship_ETA_Month",
            "Ship_MaxStaticDraught", "Ship_DestinationName",
            "Ship_VendorIdName", "Ship_VendorIdSerial", "Ship_VendorIdModel",
            "Safety_Text", "Safety_DestinationID", "Safety_Retransmission"
        };

        private readonly ClickHouseClient chClient;
        private readonly ConcurrentQueue<object[]> queue;

        private readonly int minBatchSize;
        private readonly uint histSessionId;
        private TimeSpan batchInterval;
        private int queueCount;

        private DateTime sessionStart;
        private bool disposedValue;
        
        public ClickHouseConsumer(IConfiguration configuration, ClickHouseClient chClient) : base(configuration)
        {
            this.chClient = chClient;
            this.queue = new ConcurrentQueue<object[]>();

            this.minBatchSize = configuration.GetValue<int>("Clickhouse:MinBatchSize");

            double historyIntervalSec = configuration.GetValue<double>("Clickhouse:BatchIntervalSec");
            this.batchInterval = TimeSpan.FromSeconds(historyIntervalSec);
            
            object sesId = chClient.ExecuteScalarAsync("select * from AIS_SessionsAmount").Result;
            this.histSessionId = sesId == null ? 0 : (uint)sesId + 1;
        }

        public override async Task ConsumingLoop()
        {
            this.sessionStart = DateTime.UtcNow;

            using (PeriodicTimer timer = new PeriodicTimer(batchInterval))
            {
                while (await timer.WaitForNextTickAsync())
                {
                    while (queueCount < minBatchSize) { /* wait to fill... */ }

                    await chClient.InsertBinaryAsync("AIS_History", historyColumns, queue);

                    queue.Clear();
                    Interlocked.Add(ref queueCount, queueCount * -1);
                }
            }
        }

        public void CollectEntry(AISResult aisRes)
        {
            object[] entry = ConvertAisToObject(aisRes);
            entry[0] = histSessionId;
            entry[1] = sessionStart;
            entry[2] = (long)((DateTime.UtcNow - sessionStart).TotalMilliseconds);

            queue.Enqueue(entry);
            Interlocked.Increment(ref queueCount);
        }

        private static object[] ConvertAisToObject(AISResult aismsg)
        {
            object[] result = new object[36];

            result[3] = aismsg.UserMMSI;
            result[4] = aismsg.Repeated;

            if (aismsg.Position != null)
            {
                result[5] = 1;

                result[6] = aismsg.Position.Latitude;
                result[7] = aismsg.Position.Longitude;
                result[8] = aismsg.Position.PositionAccuracy;
                result[9] = aismsg.Position.NavigationalStatus;
                result[10] = aismsg.Position.RateOfTurn;
                result[11] = aismsg.Position.Cog;
                result[12] = aismsg.Position.Sog;
                result[13] = aismsg.Position.TrueHeading;
                result[14] = aismsg.Position.TimeStopSeconds;
                result[15] = aismsg.Position.SpecialManeuver;
            }
            else if (aismsg.ShipData != null)
            {
                result[5] = 5;

                result[16] = aismsg.ShipData.Name;
                result[17] = aismsg.ShipData.ShipType;
                result[18] = aismsg.ShipData.DimA;
                result[19] = aismsg.ShipData.DimB;
                result[20] = aismsg.ShipData.DimC;
                result[21] = aismsg.ShipData.DimD;

                result[22] = aismsg.ShipData.CallSign;
                result[23] = aismsg.ShipData.IMONumber;

                result[24] = aismsg.ShipData.EtaMinute;
                result[25] = aismsg.ShipData.EtaHour;
                result[26] = aismsg.ShipData.EtaDay;
                result[27] = aismsg.ShipData.EtaMonth;

                result[28] = aismsg.ShipData.MaxStaticDraught;
                result[29] = aismsg.ShipData.Destination;

                result[30] = aismsg.ShipData.VendorIdName;
                result[31] = aismsg.ShipData.VendorIdSerial;
                result[32] = aismsg.ShipData.VendorIdModel;
            }
            else //if safety != null
            {
                result[5] = 12;

                result[33] = aismsg.Safety.Text;
                result[34] = aismsg.Safety.DestinationMMSI;
                result[35] = aismsg.Safety.Retransmission;
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    chClient.Dispose();
                    queue.Clear();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
