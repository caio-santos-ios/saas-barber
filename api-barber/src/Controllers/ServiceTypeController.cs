using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Mvc;
using api_barber.src.Requests;
using System.Threading.Tasks;
namespace api_barber.Controllers
{
    [ApiController]
    [Route("services_types")]
    public class ServiceTypeController(IServiceTypeService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<System.Collections.Generic.IEnumerable<ServiceType>> response = await service.GetAllAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<ServiceType> response = await service.GetByIdAsync(id, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object request)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<ServiceType> response = await service.CreateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] object request)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<ServiceType> response = await service.UpdateAsync(id, request, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<ServiceType> response = await service.SoftDeleteAsync(id, barbershopId, "admin");
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}



