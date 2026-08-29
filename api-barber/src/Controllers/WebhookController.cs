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
            string expectedToken = Environment.GetEnvironmentVariable("ASAAS_WEBHOOK_TOKEN") ?? Environment.GetEnvironmentVariable("WEBHOOK_TOKEN") ?? "";

            if (!string.IsNullOrEmpty(expectedToken))
            {
                string incomingToken = Request.Headers["asaas-access-token"].FirstOrDefault()
                    ?? Request.Headers["access_token"].FirstOrDefault()
                    ?? Request.Headers["X-Webhook-Token"].FirstOrDefault()
                    ?? Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "")
                    ?? "";

                if (string.IsNullOrEmpty(incomingToken) || !string.Equals(incomingToken.Trim(), expectedToken.Trim(), StringComparison.Ordinal))
                {
                    return StatusCode(401, new { Message = "Token de autenticação do webhook inválido ou não fornecido." });
                }
            }

            ResponseApi<object> response = await webhookService.HandleAsaasWebhookAsync(payload);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}
