using api_barber.Interfaces;
using api_barber.Models.Enums;
using api_barber.Requests.Subscription;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("subscriptions")]
    public class SubscriptionController(ISubscriptionService subscriptionService) : ControllerBase
    {
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            var response = await subscriptionService.CheckoutAsync(request, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices()
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            var response = await subscriptionService.GetHistoryAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpDelete("cancel")]
        public async Task<IActionResult> Cancel()
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            var response = await subscriptionService.CancelAsync(barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}


