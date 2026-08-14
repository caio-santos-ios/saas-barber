using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Mvc;
using api_barber.src.Requests;
using System.Threading.Tasks;
namespace api_barber.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController(IUserService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string barbershopId, [FromQuery] string role = null)
        {
            ResponseApi<System.Collections.Generic.IEnumerable<User>> response = await service.GetAllAsync(barbershopId, role);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, [FromQuery] string barbershopId)
        {
            ResponseApi<User> response = await service.GetByIdAsync(id, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User request, [FromQuery] string barbershopId)
        {
            if (string.IsNullOrEmpty(request.BarbershopId)) request.BarbershopId = barbershopId;
            ResponseApi<User> response = await service.CreateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] User request, [FromQuery] string barbershopId)
        {
            ResponseApi<User> response = await service.UpdateAsync(id, request, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] string barbershopId)
        {
            ResponseApi<User> response = await service.SoftDeleteAsync(id, barbershopId, "admin");
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}

