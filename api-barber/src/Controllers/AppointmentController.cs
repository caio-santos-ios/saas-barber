using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Mvc;
using api_barber.Requests.Appointment;
using api_barber.src.Requests;
using System.Threading.Tasks;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("appointments")]
    public class AppointmentController(IAppointmentService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string barbershopId)
        {
            ResponseApi<System.Collections.Generic.IEnumerable<Appointment>> response = await service.GetAllAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability([FromQuery] string barberId, [FromQuery] System.DateTime date, [FromQuery] string barbershopId)
        {
            ResponseApi<System.Collections.Generic.IEnumerable<string>> response = await service.GetAvailableSlotsAsync(barberId, date, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, [FromQuery] string barbershopId)
        {
            ResponseApi<Appointment> response = await service.CreateAsync(request, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateAppointmentStatusRequest request, [FromQuery] string barbershopId)
        {
            ResponseApi<Appointment> response = await service.UpdateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] string barbershopId)
        {
            ResponseApi<Appointment> response = await service.SoftDeleteAsync(id, barbershopId, "admin");
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}
