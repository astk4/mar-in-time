using MarInTime.Application.Repositories;
using MarInTime.Domain;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using MarInTime.Infrastructure.Persistence;
using MarInTime.Infrastructure.TransportModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;

namespace MarInTime.Infrastructure.Repositories
{
    public class EconomicZoneRepository : Repository<MainDbContext>, IGisRepository, IEconomicZoneRepository
    {
        private const string EezChunkCommand = @"select gid, ST_AsGeoJSON(geom)
                                                 FROM {0}
                                                 WHERE ST_Intersects(geom, ST_MakeEnvelope(@xMin, @yMin, @xMax, @yMax, 4326))";

        readonly IConfiguration configuration;
        public EconomicZoneRepository(MainDbContext context,IConfiguration configuration) 
            : base(context) 
        { 
            this.configuration = configuration;
        }
        public IEnumerable<string> GetRelationNames(string funcName)
        {
            return context.Database.SqlQueryRaw<string>($"select * from {funcName}();").ToList();
        }

        public async Task<IReadOnlyList<ExclusiveEconomicZone>> GetZones(ISpecification<ExclusiveEconomicZone> specification)
        {
            return await context.ExclusiveEconomicZones.Where(specification.IsSatisfiedBy)
                                                       .ToAsyncEnumerable()
                                                       .ToListAsync();
        }

        public async IAsyncEnumerable<SpatialEntityDisplayDto> GetChunksInBounds(string tableName, double xMin, double yMin, double xMax, double yMax)
        {
            string connStr = configuration["Data:Main"]!;
            using(NpgsqlConnection connection = new NpgsqlConnection(connStr))
            {
                await connection.OpenAsync();

                string cmdText = string.Format(EezChunkCommand, tableName);

                using (NpgsqlCommand selectCommand = new NpgsqlCommand(cmdText, connection))
                {
                    selectCommand.Parameters.AddWithValue("@xMin", xMin);
                    selectCommand.Parameters.AddWithValue("@xMax", xMax);
                    selectCommand.Parameters.AddWithValue("@yMin", yMin);
                    selectCommand.Parameters.AddWithValue("@yMax", yMax);

                    using (NpgsqlDataReader reader = await selectCommand.ExecuteReaderAsync())
                    {
                        try
                        {
                            while (await reader.ReadAsync())
                            {
                                yield return new SpatialEntityDisplayDto(reader.GetInt32(0), reader.GetString(1));
                            }
                        }
                        finally
                        {
                            await connection.CloseAsync();

                            await reader.DisposeAsync();
                            await selectCommand.DisposeAsync();
                            await connection.DisposeAsync();
                        }
                    }
                }
            }
        }
    }
}
