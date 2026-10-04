using MarInTime.Application.Mappers;
using MarInTime.Application.Repositories;
using MarInTime.Domain;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MarInTime.Presentation.Controllers
{
    /// <summary>
    /// For getting point objects
    /// </summary>
    [ApiController]
    [Route("locations")]
    public class LocationsController : ControllerBase
    {
        private readonly IPortRepository portRepo;

        public LocationsController(IPortRepository portRepo) => this.portRepo = portRepo;

        /// <summary>
        /// Gets all ports in the database (for loading them all on the map)
        /// </summary>
        [HttpGet]
        [Route("ports")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<PortDto>))]
        public async Task<IEnumerable<PortDto>> GetAllPorts()
        {
            IReadOnlyList<Port> ports = await portRepo.GetPortsWithCountriesRoutes(new Specification<Port>(p => true));

            return ports.Select(p => p.ToDto());
        }
    }
}
