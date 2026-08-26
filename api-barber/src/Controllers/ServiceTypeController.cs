using api_barber.Interfaces;
using Microsoft.AspNetCore.Mvc;
using api_barber.src.Requests;
using api_barber.Requests.ServiceType;
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
        public async Task<IActionResult> Create([FromBody] CreateServiceTypeRequest request)
        {
            request.BarbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            request.CreatedBy = User.FindFirst("userId")?.Value ?? "";
            var response = await service.CreateAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateServiceTypeRequest request)
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
