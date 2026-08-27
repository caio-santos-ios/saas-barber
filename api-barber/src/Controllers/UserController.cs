using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Mvc;
using api_barber.src.Requests;
using api_barber.Requests.User;
namespace api_barber.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController(IUserService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<List<dynamic>> response = await service.GetAllAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ResponseApi<User> response = await service.GetByIdAsync(id);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpGet("barbers")]
        public async Task<IActionResult> GetBarbers()
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<List<dynamic>> response = await service.GetBarbersAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomer()
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<List<dynamic>> response = await service.GetCustomersAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            request.BarbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            request.CreatedBy = User.FindFirst("userId")?.Value ?? "";
            ResponseApi<User> response = await service.CreateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
        {
            request.BarbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            request.UpdatedBy = User.FindFirst("userId")?.Value ?? "";
            ResponseApi<User> response = await service.UpdateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            string userId = User.FindFirst("userId")?.Value ?? "";
            DeleteRequest request = new()
            {
                Id = id,
                DeletedBy = userId
            };
            ResponseApi<User> response = await service.DeleteAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPatch("{id}/password")]
        public async Task<IActionResult> UpdatePassword(string id, [FromBody] ChangeUserPasswordRequest request)
        {
            ResponseApi<User> response = await service.UpdatePasswordAsync(id, request.Password);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}