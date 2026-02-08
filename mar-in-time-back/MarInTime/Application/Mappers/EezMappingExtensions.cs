using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;

namespace MarInTime.Application.Mappers
{
    public static class EezMappingExtensions
    {
        private static string FullTerritoryName(string terName, string? sovName)
        {
            if (sovName == terName || sovName == null)
            {
                return terName;
            }
            return $"{terName} ({sovName})";
        }

        public static EconomicZoneDto ToDto(this ExclusiveEconomicZone entity)
        {
            List<string> territories = new List<string>(3);
            territories.Add(FullTerritoryName(entity.Territory, entity.Sovereign));
            if (entity.Territory2 != null)
            {
                territories.Add(FullTerritoryName(entity.Territory2, entity.Sovereign2));
            }
            if (entity.Territory3 != null)
            {
                territories.Add(FullTerritoryName(entity.Territory3, entity.Sovereign3));
            }

            return new EconomicZoneDto(entity.Name, entity.Type, territories.ToArray());
        }
    }
}
