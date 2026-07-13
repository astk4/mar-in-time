using MarInTime.Application.Repositories;
using MarInTime.Application.Services;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using MarInTime.Presentation.ViewModels;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace MarInTime.Presentation.Controllers
{
    [ApiController]
    [Route("spatial")]
    public class SpatialEntitiesController : ControllerBase
    {
        private const string ExpiryKey = "Redis:ExpiryMinutes:GeoJSON";

        private readonly IGisService<EconomicZoneDto> eezService;
        private readonly ILandChecker landChecker;
        private readonly StackExchange.Redis.IDatabase redisDb;
        private readonly IConfiguration configuration;
        private ILogger<SpatialEntitiesController> logger;

        public SpatialEntitiesController(IGisService<EconomicZoneDto> eezService, 
                                         ILandChecker landChecker, 
                                         IConnectionMultiplexer multiplexer,
                                         IConfiguration configuration,
                                         ILogger<SpatialEntitiesController> logger)
        {
            this.eezService = eezService;
            this.landChecker = landChecker;
            this.redisDb = multiplexer.GetDatabase();
            this.configuration = configuration;
            this.logger = logger;
        }

        private async IAsyncEnumerable<object> StreamMapChanges(int zoom, ViewportBoundsViewModel viewModel, RedisValue[] oldChunkIds, HashSet<int> oldChunkIdsMutable, string sessionKey)
        {
            await foreach (var eez in eezService.GetZone(zoom, viewModel.West, viewModel.South, viewModel.East, viewModel.North))
            {
                if (eez == null || eez.ChunkId == null)
                {
                    continue;
                }

                if (oldChunkIds.Contains(eez.ChunkId.Value))
                {
                    oldChunkIdsMutable.Remove(eez.ChunkId.Value);
                }
                else
                {
                    double minutes = configuration.GetValue<double>(ExpiryKey);

                    await redisDb.SetAddAsync(sessionKey, eez.ChunkId.Value);
                    await redisDb.KeyExpireAsync(sessionKey, TimeSpan.FromMinutes(minutes));
                    Debug.WriteLine($"chunk {eez.ChunkId} is new");
                    yield return eez;
                }
            }

            await foreach (var chunkLeft in oldChunkIdsMutable.ToAsyncEnumerable())
            {
                await redisDb.SetRemoveAsync(sessionKey, chunkLeft);
                Debug.WriteLine($"chunk {chunkLeft} is deleted");
                yield return new { chunkId = Convert.ToInt32(chunkLeft), delete = true };
            }
        }

        [HttpGet]
        [Route("eez")]
        public async Task GetAllEconomicalZonesForMap(int zoom, int? prevZoom, [FromQuery]ViewportBoundsViewModel viewModel)
        {
            bool tierEquals = prevZoom.HasValue && eezService.ZoomTierEquals(zoom, prevZoom.Value);
            if (tierEquals && zoom > prevZoom)
            {
                Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            string sessionEezKey = $"{this.HttpContext.Session.Id}:eez";
            logger.LogInformation(sessionEezKey);
            Debug.WriteLine(sessionEezKey);

            RedisValue[] oldChunkIds;
            if (prevZoom.HasValue && !tierEquals)
            {
                await redisDb.KeyDeleteAsync(sessionEezKey);
                oldChunkIds = Array.Empty<RedisValue>();

                Response.Headers.Append("Marintime-Zoom-Tier-Change", "1");
                Response.Headers.Append("Access-Control-Expose-Headers", "Marintime-Zoom-Tier-Change");
            }
            else {
                oldChunkIds = await redisDb.SetMembersAsync(sessionEezKey);
            }
            HashSet<int> olcChunkIdsMutable = oldChunkIds.Select(x => Convert.ToInt32(x)).ToHashSet();
            if (oldChunkIds.Length == 0)
            {
                this.HttpContext.Session.Set("save_id", new byte[] { 1 });
            }

            this.HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
            Response.ContentType = "application/x-ndjson";

            await foreach (object obj in StreamMapChanges(zoom, viewModel, oldChunkIds, olcChunkIdsMutable, sessionEezKey))
            {
                await Response.WriteAsync(JsonSerializer.Serialize(obj));
                await Response.WriteAsync("\n");
                await Response.Body.FlushAsync();
            }
        }

        [HttpGet]
        [Route("point")]
        public async Task<ZonesAtPointViewModel> GetZonesListAtSeaPoint(double lng, double lat)
        {
            if (await landChecker.IsOnLand(lng, lat))
            {
                return new ZonesAtPointViewModel() { AtSea = false };
            }

            var resultViewModel = new ZonesAtPointViewModel() { AtSea = true };

            resultViewModel.ExclusiveEconomicZone = await eezService.TryGetAtCoordinates(lng, lat);
            return resultViewModel;
        }
    }
}
