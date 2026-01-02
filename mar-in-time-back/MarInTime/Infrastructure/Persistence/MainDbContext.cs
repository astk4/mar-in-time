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
        public DbSet<ExclusiveEconomicZone> ExclusiveEconomicZones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            string[] convertProps = typeof(ExclusiveEconomicZone).GetProperties()
                                                                 .Where(p => p.PropertyType == typeof(int) || p.PropertyType == typeof(int?))
                                                                 .Select(p => p.Name)
                                                                 .Where(s => s != nameof(ExclusiveEconomicZone.GID))
                                                                 .ToArray();
            foreach(string propName in convertProps)
            {
                modelBuilder.Entity<ExclusiveEconomicZone>()
                            .Property(propName)
                            .HasConversion<double>();
            } 

        }
    }
}
