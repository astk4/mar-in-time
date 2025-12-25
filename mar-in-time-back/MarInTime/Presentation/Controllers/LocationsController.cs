using MarInTime.Application.Mappers;
using MarInTime.Application.Repositories;
using MarInTime.Domain;
using MarInTime.Domain.DTOs;
using MarInTime.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MarInTime.Presentation.Controllers
{
    [ApiController]
    [Route("locations")]
    public class LocationsController : ControllerBase
    {
        private readonly IPortRepository portRepo;

        public LocationsController(IPortRepository portRepo) => this.portRepo = portRepo;

        [HttpGet]
        [Route("ports")]
        public async Task<IEnumerable<PortDto>> GetAllPorts()
        {
            IReadOnlyList<Port> ports = await portRepo.GetPortsWithCountriesRoutes(new Specification<Port>(p => true));

            return ports.Select(p => p.ToDto());
        }
    }
}
