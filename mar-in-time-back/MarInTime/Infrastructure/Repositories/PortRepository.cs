using MarInTime.Application.Repositories;
using MarInTime.Domain;
using MarInTime.Domain.Entities;
using MarInTime.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarInTime.Infrastructure.Repositories
{
    public class PortRepository : Repository<MainDbContext>, IPortRepository
    {
        public PortRepository(MainDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Port>> GetPortsWithCountriesRoutes(ISpecification<Port> specification)
        {
            return await context.Ports.Include(p => p.Country)
                                      .Include(p => p.Routes)
                                      .Where(specification.IsSatisfiedBy)
                                      .ToAsyncEnumerable()
                                      .ToListAsync();
        }
    }
}
