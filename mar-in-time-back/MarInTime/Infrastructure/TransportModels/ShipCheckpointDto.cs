using MessagePack;

namespace MarInTime.Infrastructure.TransportModels
{
    [MessagePackObject]
    public class ShipCheckpointDto
    {
        public const string HashKey = "ships_data",
                    LatitudeHashField = "lat",
                    LongitudeHashField = "long",
                    TypeHashField = "type",
                    CourseHashField = "course";

        [Key(0)]
        public int MMSI { get; set; }

        [Key(1)]
        public double Latitude { get; set; }
        [Key(2)]
        public double Longitude { get; set; }

        [Key(3)]
        public int TypeId { get; set; } = 21; //= ship type other

        [Key(4)]
        public double Course { get; set; } = 360; //unavailable by default

        public static ShipType GetTypeForNumber(int aisNum)
        {
            switch (aisNum)
            {
                case 30:
                    return ShipType.Fishing;

                case 33:
                    return ShipType.Dredging;
                case 34:
                    return ShipType.Diving;
                case 35:
                    return ShipType.Military;
                case 36:
                    return ShipType.Sail;
                case 37:
                    return ShipType.Pleasure;

                case 50:
                    return ShipType.Pilot;
                case 51:
                    return ShipType.SearchRescue;
                case 52:
                    return ShipType.Tug;
                case 53:
                    return ShipType.PortTender;
                case 54:
                    return ShipType.AntiPollution;
                case 55:
                    return ShipType.Law;

                case 58:
                    return ShipType.Medical;
                case 59:
                    return ShipType.RR18;
            }

            if (aisNum <= 19 || aisNum >= 38 && aisNum <= 39)
            {
                return ShipType.Other;
            }

            if (aisNum <= 29)
            {
                return ShipType.WIG;
            }
            if (aisNum <= 32)
            {
                return ShipType.Towing;
            }
            if (aisNum <= 49)
            {
                return ShipType.HSC;
            }
            if (aisNum <= 57)
            {
                return ShipType.Local;
            }
            if (aisNum <= 69)
            {
                return ShipType.Passenger;
            }
            if (aisNum <= 79)
            {
                return ShipType.Cargo;
            }
            if (aisNum <= 89)
            {
                return ShipType.Tanker;
            }

            return ShipType.Other;
        }
    }
}
