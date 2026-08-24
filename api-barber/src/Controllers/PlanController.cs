using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Mvc;
using api_barber.src.Requests;
using api_barber.Requests.Plan;
using System.Threading.Tasks;
namespace api_barber.Controllers
{
    [ApiController]
    [Route("plans")]
    public class PlanController(IPlanService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            var response = await service.GetAllAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var response = await service.GetByIdAsync(id);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlanRequest request)
        {
            request.BarbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            request.CreatedBy = User.FindFirst("userId")?.Value ?? "";
            var response = await service.CreateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePlanRequest request)
        {
            request.BarbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            request.UpdatedBy = User.FindFirst("userId")?.Value ?? "";
            var response = await service.UpdateAsync(request);
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
            var response = await service.DeleteAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}
