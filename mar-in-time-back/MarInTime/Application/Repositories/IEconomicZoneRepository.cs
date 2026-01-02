using MarInTime.Domain;
using MarInTime.Domain.Entities;
using MarInTime.Infrastructure.TransportModels;

namespace MarInTime.Application.Repositories
{
    public interface IEconomicZoneRepository
    {
        public Task<IReadOnlyList<ExclusiveEconomicZone>> GetZones(ISpecification<ExclusiveEconomicZone> specification);
        public IAsyncEnumerable<EconomicZoneDisplayDto> GetZonesTransport(ISpecification<EconomicZoneDisplayDto> specification);
    }
}
