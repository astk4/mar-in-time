using MarInTime.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Route = MarInTime.Domain.Entities.Route;

namespace MarInTime.Infrastructure.Persistence
{
    public class MainDbContext : DbContext
    {
        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Port> Ports { get; set; }
        public DbSet<Route> Routes { get; set; }
    }
}
