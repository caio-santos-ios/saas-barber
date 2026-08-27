using api_barber.Interfaces;
using api_barber.Requests.Dashboard;
using api_barber.src.Requests;
using Microsoft.AspNetCore.Mvc;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("web/dashboard")]
    public class DashboardController(IDashboardService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMetrics([FromQuery] DashboardQueryRequest query)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<DashboardMetricsResponse> response = await service.GetMetricsAsync(barbershopId, query);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}


