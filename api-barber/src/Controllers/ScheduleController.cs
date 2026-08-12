using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using api_barber.Requests.Schedule;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("schedules")]
    public class ScheduleController : ControllerBase
    {
        private readonly IBaseRepository<Schedule> _repo;

        public ScheduleController(IBaseRepository<Schedule> repo)
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
        public async Task<IActionResult> Create([FromBody] CreateScheduleRequest request, [FromQuery] string barbershopId)
        {
            var entity = new Schedule
            {
                Day = request.Day,
                StartHour = request.StartHour,
                EndHour = request.EndHour,
                IntervalMinutes = request.IntervalMinutes,
                Notes = request.Notes,
                BarberId = request.BarberId,
                BarbershopId = barbershopId
            };
            await _repo.CreateAsync(entity);
            return Ok(entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateScheduleRequest request, [FromQuery] string barbershopId)
        {
            var entity = await _repo.GetByIdAsync(id, barbershopId);
            if (entity == null) return NotFound();

            entity.StartHour = request.StartHour;
            entity.EndHour = request.EndHour;
            entity.IntervalMinutes = request.IntervalMinutes;
            entity.Notes = request.Notes;
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
