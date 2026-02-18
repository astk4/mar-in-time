using MessagePack;

namespace MarInTime.Infrastructure.TransportModels
{
    [MessagePackObject]
    public class ShipPositionCheckpoint
    {
        public const string HashKey = "ships_pos";
        [Key(0)]
        public int MMSI { get; set; }

        [Key(1)]
        public double Latitude { get; set; }
        [Key(2)]
        public double Longitude { get; set; }

        //ship type will be here in the future
    }
}
