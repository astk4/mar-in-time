using MarInTime.Application.Repositories;
using MarInTime.Domain;
using MarInTime.Domain.Entities;
using MarInTime.Infrastructure.Persistence;
using MarInTime.Infrastructure.TransportModels;
using Npgsql;

namespace MarInTime.Infrastructure.Repositories
{
    public class EconomicZoneRepository : Repository<MainDbContext>, IEconomicZoneRepository
    {
        private const string EezViewSelectCommand = "SELECT gid, geoname, x_1, y_1, geom_geojson FROM eez_v12_geojson";

        readonly IConfiguration configuration;
        public EconomicZoneRepository(MainDbContext context,IConfiguration configuration) 
            : base(context) 
        { 
            this.configuration = configuration;
        }

        public async Task<IReadOnlyList<ExclusiveEconomicZone>> GetZones(ISpecification<ExclusiveEconomicZone> specification)
        {
            return await context.ExclusiveEconomicZones.Where(specification.IsSatisfiedBy)
                                                       .ToAsyncEnumerable()
                                                       .ToListAsync();
        }

        public async IAsyncEnumerable<EconomicZoneDisplayDto> GetZonesTransport(ISpecification<EconomicZoneDisplayDto> specification)
        {
            string connStr = configuration["Data:Main"]!;
            using(NpgsqlConnection connection = new NpgsqlConnection(connStr))
            {
                await connection.OpenAsync();

                using(NpgsqlCommand selectCommand = new NpgsqlCommand(EezViewSelectCommand, connection))
                {
                    using (NpgsqlDataReader reader = await selectCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dto = new EconomicZoneDisplayDto(reader.GetInt32(0),
                                                                 reader.GetString(1),
                                                                 reader.GetString(4),
                                                                 reader.GetDouble(2),
                                                                 reader.GetDouble(3));
                            if (!specification.IsSatisfiedBy(dto))
                            {
                                continue;
                            }
                            yield return dto;
                        }
                    }
                }
                await connection.CloseAsync();
            }
        }
    }
}
