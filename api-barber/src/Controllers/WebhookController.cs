using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("webhooks/asaas")]
    public class WebhookController : ControllerBase
    {
        private readonly IBaseRepository<Barbershop> _barbershopRepo;

        public WebhookController(IBaseRepository<Barbershop> barbershopRepo)
        {
            _barbershopRepo = barbershopRepo;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] JsonElement payload)
        {
            try
            {
                var eventObj = payload.GetProperty("event").GetString();
                var payment = payload.GetProperty("payment");
                var customerId = payment.GetProperty("customer").GetString();

                // Identifica a barbearia pelo AsaasCustomerId
                var barbershops = await _barbershopRepo.GetAllAsync(string.Empty);
                var barbershop = barbershops.FirstOrDefault(b => b.AsaasCustomerId == customerId);

                if (barbershop != null)
                {
                    if (eventObj == "PAYMENT_RECEIVED")
                    {
                        barbershop.SubscriptionStatus = SubscriptionStatusEnum.Ativa;
                        await _barbershopRepo.UpdateAsync(barbershop.Id, barbershop);
                    }
                    else if (eventObj == "PAYMENT_OVERDUE")
                    {
                        barbershop.SubscriptionStatus = SubscriptionStatusEnum.Inadimplente;
                        await _barbershopRepo.UpdateAsync(barbershop.Id, barbershop);
                    }
                }

                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
