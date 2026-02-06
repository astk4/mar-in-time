using MarInTime.Application.Repositories;
using MarInTime.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarInTime.Infrastructure.Repositories
{
    public class LandChecker : Repository<MainDbContext>, ILandChecker
    {
        private const string LandQueryTemplate = "SELECT EXISTS (SELECT 1 FROM land_polygons WHERE ST_Contains(geom, ST_Point({0}, {1}, 4326))) as \"Value\"";
        public LandChecker(MainDbContext context) : base(context)
        {
        }

        public async Task<bool> IsOnLand(double lng, double lat)
        {
            return await this.context.Database.SqlQueryRaw<bool>(LandQueryTemplate, lng, lat).SingleAsync();
        }
    }
}
