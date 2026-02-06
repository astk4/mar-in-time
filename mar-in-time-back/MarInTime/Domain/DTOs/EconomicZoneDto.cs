using MarInTime.Infrastructure.Converters;
using System.Text.Json.Serialization;

namespace MarInTime.Domain.DTOs
{
    public record EconomicZoneDto(string Name, string Type, string[] Territories)
    {
    }
}
