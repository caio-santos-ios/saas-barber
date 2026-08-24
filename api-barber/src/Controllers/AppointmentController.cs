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
        public async Task<IActionResult> GetAll()
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<List<dynamic>> response = await service.GetAllAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ResponseApi<Appointment> response = await service.GetByIdAsync(id);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability([FromQuery] string barberId, [FromQuery] System.DateTime date)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<List<string>> response = await service.GetAvailableSlotsAsync(barberId, date, barbershopId);
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
