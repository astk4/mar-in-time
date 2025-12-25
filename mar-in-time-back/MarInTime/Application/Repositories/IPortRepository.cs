using MarInTime.Domain;
using MarInTime.Domain.Entities;

namespace MarInTime.Application.Repositories
{
    public interface IPortRepository
    {
        public Task<IReadOnlyList<Port>> GetPortsWithCountriesRoutes(ISpecification<Port> specification);
    }
}
