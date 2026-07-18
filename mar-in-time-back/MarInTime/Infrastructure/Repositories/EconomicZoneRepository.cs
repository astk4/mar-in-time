using MarInTime.Application.Repositories;
using MarInTime.Domain;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using MarInTime.Infrastructure.Persistence;
using MarInTime.Infrastructure.TransportModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;
using Npgsql;
using System.Collections.Frozen;
using System.Data;
using System.Data.Common;

namespace MarInTime.Infrastructure.Repositories
{
    public class EconomicZoneRepository : Repository<MainDbContext>, IGisStreamRepository, ISpatialRepository<ExclusiveEconomicZone>
    {
        private const string ZoomTiersCacheKey = "zooms";
        private const string EezChunkCommand = @"select gid, chunk_id, ST_AsGeoJSON(geom)
                                                 FROM {0}
                                                 WHERE ST_Intersects(geom, ST_MakeEnvelope(@xMin, @yMin, @xMax, @yMax, 4326)) ORDER BY ",
                             orderByAreaPart = " ST_Area(geom) DESC",
                             orderByDistanceToViewportCenterPart = " distance_from_chunk_to_point(geom, @pointX, @pointY)";

        private const string GidByPointCommand = "select gid as \"Value\" from eez_v12 where ST_Intersects(geom, ST_Point({0}, {1}, 4326))";

        private readonly IMemoryCache memoryCache;

        public EconomicZoneRepository(MainDbContext context, IMemoryCache memoryCache) 
            : base(context) 
        {
            this.memoryCache = memoryCache;
        }
        public IEnumerable<string> GetRelationNames(string funcName)
        {
            FrozenSet<string>? namesSet;
            if (!memoryCache.TryGetValue(ZoomTiersCacheKey, out namesSet))
            {
                namesSet = context.Database.SqlQueryRaw<string>($"select * from {funcName}();").ToFrozenSet();
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(24));

                memoryCache.Set(ZoomTiersCacheKey, namesSet, options);
            }

            return namesSet!;
        }

        public async Task<IReadOnlyList<ExclusiveEconomicZone>> GetZonesAt(double lng, double lat)
        {
            int? gid = await context.Database.SqlQueryRaw<int?>(GidByPointCommand, lng, lat).SingleOrDefaultAsync();

            if (gid == null)
            {
                return Array.Empty<ExclusiveEconomicZone>();
            }

            return await context.ExclusiveEconomicZones.Where(new GidSpecification(gid.Value).IsSatisfiedBy)
                                                       .ToAsyncEnumerable()
                                                       .ToListAsync();
        }

        public async IAsyncEnumerable<SpatialEntityDisplayDto> GetChunksInBounds(string tableName, double xMin, double yMin, double xMax, double yMax, bool orderByArea)
        {
            await this.context.Database.OpenConnectionAsync();

            string cmdText = string.Format(EezChunkCommand, tableName);
            cmdText += orderByArea ? orderByAreaPart : orderByDistanceToViewportCenterPart;

            using (DbCommand selectCommand = this.context.Database.GetDbConnection().CreateCommand())
            {
                selectCommand.CommandText = cmdText;
                selectCommand.CommandType = CommandType.Text;

                AddSqlParameterWithValue(selectCommand, "@xMin", xMin);
                AddSqlParameterWithValue(selectCommand, "@xMax", xMax);
                AddSqlParameterWithValue(selectCommand, "@yMin", yMin);
                AddSqlParameterWithValue(selectCommand, "@yMax", yMax);

                AddSqlParameterWithValue(selectCommand, "@pointX", (xMin + xMax) / 2);
                AddSqlParameterWithValue(selectCommand, "@pointY", (yMin + yMax) / 2);

                using (DbDataReader reader = await selectCommand.ExecuteReaderAsync())
                {
                    try
                    {
                        while (await reader.ReadAsync())
                        {
                            yield return new SpatialEntityDisplayDto(reader.GetInt32(0), reader.GetFieldValue<int?>(1), reader.GetString(2));
                        }
                    }
                    finally
                    {
                        await this.context.Database.CloseConnectionAsync();

                        await reader.DisposeAsync();
                        await selectCommand.DisposeAsync();
                    }
                }
            }
        }

        private static void AddSqlParameterWithValue(DbCommand command, string name, object value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}
