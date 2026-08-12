using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using api_barber.Requests.Notification;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly IBaseRepository<Notification> _repo;

        public NotificationController(IBaseRepository<Notification> repo)
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
        public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request, [FromQuery] string barbershopId)
        {
            var entity = new Notification
            {
                Type = request.Type,
                Title = request.Title,
                Message = request.Message,
                UserId = request.UserId,
                TargetRole = request.TargetRole,
                RelatedAppointmentId = request.RelatedAppointmentId,
                BarbershopId = barbershopId
            };
            await _repo.CreateAsync(entity);
            return Ok(entity);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id, [FromQuery] string barbershopId)
        {
            var entity = await _repo.GetByIdAsync(id, barbershopId);
            if (entity == null) return NotFound();

            entity.Read = true;
            await _repo.UpdateAsync(id, entity);
            return NoContent();
        }
    }
}
