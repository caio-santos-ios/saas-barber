using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using api_barber.Requests.Barbershop;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("barbershops")]
    public class BarbershopController : ControllerBase
    {
        private readonly IBaseRepository<Barbershop> _repo;

        public BarbershopController(IBaseRepository<Barbershop> repo)
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateBarbershopRequest request, [FromQuery] string barbershopId)
        {
            var entity = await _repo.GetByIdAsync(id, barbershopId);
            if (entity == null) return NotFound();

            entity.Name = request.Name;
            entity.Phone = request.Phone;
            entity.WhatsApp = request.WhatsApp;
            entity.Logo = request.Logo;
            entity.PlanId = request.PlanId;
            entity.Active = request.Active;

            await _repo.UpdateAsync(id, entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] string barbershopId)
        {
            var entity = await _repo.GetByIdAsync(id, barbershopId);
            if (entity == null) return NotFound();
            
            await _repo.SoftDeleteAsync(id, barbershopId, "admin"); // TODO user context
            return NoContent();
        }
    }
}
