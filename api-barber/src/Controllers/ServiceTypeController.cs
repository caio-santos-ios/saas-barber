using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using api_barber.Requests.Service;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("service-types")]
    public class ServiceTypeController : ControllerBase
    {
        private readonly IBaseRepository<ServiceType> _repo;

        public ServiceTypeController(IBaseRepository<ServiceType> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string barbershopId)
        {
            var result = await _repo.GetAllAsync(barbershopId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, [FromQuery] string barbershopId)
        {
            var result = await _repo.GetByIdAsync(id, barbershopId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceTypeRequest request, [FromQuery] string barbershopId)
        {
            var entity = new ServiceType
            {
                Name = request.Name,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                Value = request.Value,
                Category = request.Category,
                BarbershopId = barbershopId
            };
            await _repo.CreateAsync(entity);
            return Ok(entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateServiceTypeRequest request, [FromQuery] string barbershopId)
        {
            var entity = await _repo.GetByIdAsync(id, barbershopId);
            if (entity == null) return NotFound();

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.DurationMinutes = request.DurationMinutes;
            entity.Value = request.Value;
            entity.Category = request.Category;
            entity.Active = request.Active;

            await _repo.UpdateAsync(id, entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] string barbershopId)
        {
            var entity = await _repo.GetByIdAsync(id, barbershopId);
            if (entity == null) return NotFound();
            
            await _repo.SoftDeleteAsync(id, barbershopId, "admin");
            return NoContent();
        }
    }
}
