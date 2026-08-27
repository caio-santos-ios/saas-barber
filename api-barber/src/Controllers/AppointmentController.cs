using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Mvc;
using api_barber.Requests.Appointment;
using api_barber.src.Requests;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("appointments")]
    public class AppointmentController(IAppointmentService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? barbershopId, [FromQuery] string? customerId, [FromQuery] string? barberId)
        {
            string effectiveBarbershopId = !string.IsNullOrEmpty(barbershopId)
                ? barbershopId
                : (User.FindFirst("barbershopId")?.Value ?? "");

            string role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            string userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("userId")?.Value 
                ?? "";

            string effectiveCustomerId = !string.IsNullOrEmpty(customerId) ? customerId : (role == "Customer" ? userId : "");
            string effectiveBarberId = !string.IsNullOrEmpty(barberId) ? barberId : (role == "Barber" ? userId : "");

            ResponseApi<List<dynamic>> response = await service.GetAllAsync(effectiveBarbershopId, effectiveCustomerId, effectiveBarberId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ResponseApi<Appointment> response = await service.GetByIdAsync(id);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability([FromQuery] string barberId, [FromQuery] System.DateTime date, [FromQuery] string? barbershopId)
        {
            string effectiveBarbershopId = !string.IsNullOrEmpty(barbershopId)
                ? barbershopId
                : (User.FindFirst("barbershopId")?.Value ?? "");
            ResponseApi<List<string>> response = await service.GetAvailableSlotsAsync(barberId, date, effectiveBarbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, [FromQuery] string? barbershopId)
        {
            request.BarbershopId = !string.IsNullOrEmpty(request.BarbershopId)
                ? request.BarbershopId
                : (!string.IsNullOrEmpty(barbershopId) ? barbershopId : (User.FindFirst("barbershopId")?.Value ?? ""));
            request.CreatedBy = User.FindFirst("userId")?.Value ?? "";
            ResponseApi<Appointment> response = await service.CreateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAppointmentRequest request, [FromQuery] string? barbershopId)
        {
            request.BarbershopId = !string.IsNullOrEmpty(request.BarbershopId)
                ? request.BarbershopId
                : (!string.IsNullOrEmpty(barbershopId) ? barbershopId : (User.FindFirst("barbershopId")?.Value ?? ""));
            request.UpdatedBy = User.FindFirst("userId")?.Value ?? "";
            ResponseApi<Appointment> response = await service.UpdateAsync(request);
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
