using MarInTime.Application.Mappers;
using MarInTime.Application.Repositories;
using MarInTime.Domain;
using MarInTime.Domain.Entities;
using MarInTime.Infrastructure.Converters;
using MarInTime.Infrastructure.TransportModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarInTime.Presentation.Controllers
{
    [ApiController]
    [Route("spatial")]
    public class SpatialEntitiesController : ControllerBase
    {
        private readonly IEconomicZoneRepository eezRepo;

        public SpatialEntitiesController(IEconomicZoneRepository eezRepo) => this.eezRepo = eezRepo;

        [HttpGet]
        [Route("eez")]
        public async Task GetAllEconomicalZonesForMap()
        {
            Response.ContentType = "application/json";

            await Response.WriteAsync("{\"type\":\"FeatureCollection\",\"features\":[");

            bool first = true;

            await foreach (var eez in eezRepo.GetZonesTransport(new Specification<EconomicZoneDisplayDto>(ee => true)))
            {
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
