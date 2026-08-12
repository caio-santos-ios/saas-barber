using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using api_barber.Requests.User;
using System.Linq;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IBaseRepository<User> _repo;

        public UserController(IBaseRepository<User> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string barbershopId, [FromQuery] string role = null)
        {
            var result = await _repo.GetAllAsync(barbershopId);
            if (!string.IsNullOrEmpty(role))
            {
                result = result.Where(u => u.Role.ToString().ToLower() == role.ToLower());
            }
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
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request, [FromQuery] string barbershopId)
        {
            var entity = await _repo.GetByIdAsync(id, barbershopId);
            if (entity == null) return NotFound();

            entity.Name = request.Name;
            entity.WhatsApp = request.WhatsApp;
            entity.DateOfBirth = request.DateOfBirth;
            entity.Photo = request.Photo;
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
