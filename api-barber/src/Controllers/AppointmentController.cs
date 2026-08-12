using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using api_barber.Requests.Appointment;
using api_barber.Models.Enums;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly IBaseRepository<Appointment> _repo;

        public AppointmentController(IBaseRepository<Appointment> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string barbershopId)
        {
            var result = await _repo.GetAllAsync(barbershopId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, [FromQuery] string barbershopId)
        {
            var entity = new Appointment
            {
                Date = request.Date,
                Hour = request.Hour,
                Notes = request.Notes,
                BarberId = request.BarberId,
                CustomerId = request.CustomerId,
                ServiceId = request.ServiceId,
                ServiceTypeId = request.ServiceTypeId,
                CustomerName = request.CustomerName,
                ServiceTypeName = request.ServiceTypeName,
                Value = request.Value,
                Status = AppointmentStatusEnum.Marcado,
                BarbershopId = barbershopId
            };
            await _repo.CreateAsync(entity);
            return Ok(entity);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateAppointmentStatusRequest request, [FromQuery] string barbershopId)
        {
            var entity = await _repo.GetByIdAsync(id, barbershopId);
            if (entity == null) return NotFound();

            entity.Status = request.Status;
            if (request.Status == AppointmentStatusEnum.Cancelado)
            {
                entity.CancelNotes = request.CancelNotes;
            }

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
