using api_barber.Interfaces;
using api_barber.Models.Enums;
using api_barber.src.Requests;
using System.Text.Json;
namespace api_barber.Services
{
    public class WebhookService(IBarbershopService barbershopService) : IWebhookService
    {
        public async Task<ResponseApi<object>> HandleAsaasWebhookAsync(JsonElement payload)
        {
            try
            {
                var eventObj = payload.GetProperty("event").GetString();
                var payment = payload.GetProperty("payment");
                var customerId = payment.GetProperty("customer").GetString();
                var barbershopsResponse = await barbershopService.GetAllAsync(string.Empty);
                var barbershop = barbershopsResponse.Data?.FirstOrDefault(b => b.AsaasCustomerId == customerId);
                if (barbershop != null)
                {
                    if (eventObj == "PAYMENT_RECEIVED" || eventObj == "PAYMENT_CONFIRMED")
                    {
                        barbershop.SubscriptionStatus = SubscriptionStatusEnum.Ativa;
                        await barbershopService.UpdateEntityAsync(barbershop);
                    }
                    else if (eventObj == "PAYMENT_OVERDUE")
                    {
                        barbershop.SubscriptionStatus = SubscriptionStatusEnum.Inadimplente;
                        await barbershopService.UpdateEntityAsync(barbershop);
                    }
                }
                return new(null, 200, "Webhook processado");
            }
            catch
            {
                return new(null, 400, "Erro ao processar webhook");
            }
        }
    }
}



