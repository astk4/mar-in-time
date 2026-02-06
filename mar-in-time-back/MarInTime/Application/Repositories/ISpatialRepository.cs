using MarInTime.Domain;
using MarInTime.Domain.Entities;

namespace MarInTime.Application.Repositories
{
    public interface ISpatialRepository<T> where T : SpatialEntity
    {
        public Task<IReadOnlyList<T>> GetZones(ISpecification<T> specification);
    }
}
