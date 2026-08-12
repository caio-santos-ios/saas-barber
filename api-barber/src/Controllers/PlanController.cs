using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using api_barber.Requests.Plan;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("plans")]
    public class PlanController : ControllerBase
    {
        private readonly IBaseRepository<Plan> _repo;

        public PlanController(IBaseRepository<Plan> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAllAsync(string.Empty);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _repo.GetByIdAsync(id, string.Empty);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlanRequest request)
        {
            var plan = new Plan
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Level = request.Level
            };
            await _repo.CreateAsync(plan);
            return Ok(plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdatePlanRequest request)
        {
            var entity = await _repo.GetByIdAsync(id, string.Empty);
            if (entity == null) return NotFound();

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Price = request.Price;
            entity.Level = request.Level;
            entity.Active = request.Active;

            await _repo.UpdateAsync(id, entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _repo.GetByIdAsync(id, string.Empty);
            if (entity == null) return NotFound();
            
            await _repo.SoftDeleteAsync(id, string.Empty, "admin");
            return NoContent();
        }
    }
}
