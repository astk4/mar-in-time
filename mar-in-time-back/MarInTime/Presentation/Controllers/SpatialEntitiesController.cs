using MarInTime.Application.Services;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using MarInTime.Presentation.ViewModels;
using Microsoft.AspNetCore.Http.Features;
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
            this.HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
            Response.ContentType = "application/x-ndjson";

            await foreach (var eez in eezService.GetZone(zoom, viewModel.West, viewModel.South, viewModel.East, viewModel.North))
            {
                if (eez == null)
                {
                    continue;
                }

                await Response.WriteAsync(JsonSerializer.Serialize(eez));
                await Response.WriteAsync("\n");
                await Response.Body.FlushAsync();

                Debug.WriteLine($"gid {eez.GID}");
            }
        }
    }
}
