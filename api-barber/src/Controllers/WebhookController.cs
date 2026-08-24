using api_barber.Interfaces;
using api_barber.src.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
namespace api_barber.Controllers
{
    [ApiController]
    [Route("webhooks/asaas")]
    public class WebhookController(IWebhookService webhookService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] JsonElement payload)
        {
            string barbershopId = User.FindFirst("barbershopId")?.Value ?? "";
            ResponseApi<object> response = await webhookService.HandleAsaasWebhookAsync(payload);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}



