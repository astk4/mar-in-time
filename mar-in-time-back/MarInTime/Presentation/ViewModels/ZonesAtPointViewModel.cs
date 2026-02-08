using MarInTime.Domain.DTOs;

namespace MarInTime.Presentation.ViewModels
{
    public class ZonesAtPointViewModel
    {
        public bool AtSea {  get; set; }

        public EconomicZoneDto? ExclusiveEconomicZone { get; set; }
    }
}
