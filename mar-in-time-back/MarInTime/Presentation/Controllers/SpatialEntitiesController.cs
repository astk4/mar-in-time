using MarInTime.Application.Services;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using MarInTime.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace MarInTime.Presentation.Controllers
{
    [ApiController]
    [Route("spatial")]
    public class SpatialEntitiesController : ControllerBase
    {
        private readonly IEezService eezService;

        public SpatialEntitiesController(IEezService eezService) => this.eezService = eezService;

        [HttpGet]
        [Route("eez")]
        public async Task GetAllEconomicalZonesForMap(int zoom, [FromQuery]ViewportBoundsViewModel viewModel)
        {
            Response.ContentType = "application/json";

            await Response.WriteAsync("{\"type\":\"FeatureCollection\",\"features\":[");

            bool first = true;

            await foreach (var eez in eezService.GetZone(zoom, viewModel.West, viewModel.South, viewModel.East, viewModel.North))
            {
                if (eez == null)
                {
                    continue;
                }

                if (!first)
                {
                    await Response.WriteAsync(",");
                }

                first = false;

                await Response.WriteAsync(JsonSerializer.Serialize(eez));
                Debug.WriteLine($"gid {eez.GID}");
            }

            await Response.WriteAsync("]}");
        }
    }
}
