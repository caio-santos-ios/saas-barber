using api_barber.Interfaces;
using api_barber.Requests.Subscription;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("invoices/{paymentId}/pix")]
        public async Task<IActionResult> GetInvoicePix(string paymentId)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            var response = await subscriptionService.GetInvoicePixAsync(paymentId, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPost("invoices/{paymentId}/pay-card")]
        public async Task<IActionResult> PayInvoiceWithCreditCard(string paymentId, [FromBody] CreditCardRequest request)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            var response = await subscriptionService.PayInvoiceWithCreditCardAsync(paymentId, request, barbershopId);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}


