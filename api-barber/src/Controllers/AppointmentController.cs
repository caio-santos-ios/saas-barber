using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Mvc;
using api_barber.Requests.Appointment;
using api_barber.src.Requests;
using Microsoft.AspNetCore.Authorization;

namespace api_barber.Controllers
{
    [ApiController]
    [Authorize]
    [Route("appointments")]
    public class AppointmentController(IAppointmentService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? customerId, [FromQuery] string? barberId)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";

            string role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            string userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("userId")?.Value 
                ?? "";

            string effectiveCustomerId = !string.IsNullOrEmpty(customerId) ? customerId : (role == "Customer" ? userId : "");
            string effectiveBarberId = !string.IsNullOrEmpty(barberId) ? barberId : (role == "Barber" ? userId : "");

            ResponseApi<List<dynamic>> response = await service.GetAllAsync(barbershopId, effectiveCustomerId, effectiveBarberId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ResponseApi<Appointment> response = await service.GetByIdAsync(id);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability([FromQuery] string barberId, [FromQuery] System.DateTime date, [FromQuery] string? serviceId, [FromQuery] string? customerId)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            string effectiveCustomerId = !string.IsNullOrEmpty(customerId) ? customerId : (User.FindFirst("userId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
            ResponseApi<List<string>> response = await service.GetAvailableSlotsAsync(barberId, date, barbershopId, serviceId, effectiveCustomerId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            request.BarbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            request.CreatedBy = User.FindFirst("userId")?.Value ?? "";
            ResponseApi<Appointment> response = await service.CreateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAppointmentRequest request)
        {
            request.BarbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            request.UpdatedBy = User.FindFirst("userId")?.Value ?? "";
            ResponseApi<Appointment> response = await service.UpdateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateAppointmentStatusRequest request)
        {
            request.UpdatedBy = User.FindFirst("userId")?.Value ?? "";
            ResponseApi<Appointment> response = await service.UpdateStatusAsync(request);
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
            ResponseApi<Appointment> response = await service.DeleteAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}
