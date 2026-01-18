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
        private readonly IEezService eezService;
        private readonly StackExchange.Redis.IDatabase redisDb;
        public SpatialEntitiesController(IEezService eezService, IConnectionMultiplexer multiplexer)
        {
            this.eezService = eezService;
            this.redisDb = multiplexer.GetDatabase();
        }

        private async IAsyncEnumerable<object> StreamMapChanges(int zoom, ViewportBoundsViewModel viewModel, RedisValue[] oldGids, ISet<int> oldGidsMutable, string sessionKey)
        {
            await foreach (var eez in eezService.GetZone(zoom, viewModel.West, viewModel.South, viewModel.East, viewModel.North))
            {
                if (eez == null)
                {
                    continue;
                }

                if (oldGids.Contains(eez.GID))
                {
                    oldGidsMutable.Remove(eez.GID);
                }
                else
                {
                    await redisDb.SetAddAsync(sessionKey, eez.GID);
                    Debug.WriteLine($"gid {eez.GID} is new");
                    yield return eez;
                }
            }

            await foreach (var gidLeft in oldGidsMutable.ToAsyncEnumerable())
            {
                await redisDb.SetRemoveAsync(sessionKey, gidLeft);
                Debug.WriteLine($"gid {gidLeft} is deleted");
                yield return new { gid = Convert.ToInt32(gidLeft), delete = true };
            }
        }

        [HttpGet]
        [Route("eez")]
        public async Task GetAllEconomicalZonesForMap(int zoom, int? prevZoom, [FromQuery]ViewportBoundsViewModel viewModel)
        {
            if (prevZoom.HasValue && zoom > prevZoom && eezService.ZoomTierEquals(zoom, prevZoom.Value))
            {
                Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            string sessionEezKey = $"{this.HttpContext.Session.Id}:eez";
            Debug.WriteLine(sessionEezKey);

            RedisValue[] oldGids;
            if (prevZoom.HasValue && zoom > prevZoom)
            {
                await redisDb.KeyDeleteAsync(sessionEezKey);
                oldGids = new RedisValue[] { };
            }
            else {
                oldGids = await redisDb.SetMembersAsync(sessionEezKey);
            }
            HashSet<int> oldGidsMutable = oldGids.Select(x => Convert.ToInt32(x)).ToHashSet();
            if (oldGids.Length == 0)
            {
                this.HttpContext.Session.Set("save_id", new byte[] { 1 });
            }

            this.HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
            Response.ContentType = "application/x-ndjson";

            await foreach (object obj in StreamMapChanges(zoom, viewModel, oldGids, oldGidsMutable, sessionEezKey))
            {
                await Response.WriteAsync(JsonSerializer.Serialize(obj));
                await Response.WriteAsync("\n");
                await Response.Body.FlushAsync();
            }
        }
    }
}
