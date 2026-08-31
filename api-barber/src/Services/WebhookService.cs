using api_barber.Interfaces;
using api_barber.Models;
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
                string eventName = payload.TryGetProperty("event", out var eventProp) ? (eventProp.GetString() ?? "") : "";
                
                string customerId = "";
                if (payload.TryGetProperty("payment", out var paymentProp))
                {
                    if (paymentProp.TryGetProperty("customer", out var custProp))
                    {
                        customerId = custProp.GetString() ?? "";
                    }
                }

                if (string.IsNullOrEmpty(customerId))
                {
                    return new(null, 200, "Webhook recebido sem identificador de cliente");
                }

                var barbershopResponse = await barbershopService.GetByAsaasCustomerIdAsync(customerId);
                Barbershop? barbershop = barbershopResponse.Data;

                if (barbershop != null)
                {
                    if (eventName is "PAYMENT_RECEIVED" or "PAYMENT_CONFIRMED" or "PAYMENT_RESTORED")
                    {
                        barbershop.SubscriptionStatus = SubscriptionStatusEnum.Ativa;
                        await barbershopService.UpdateEntityAsync(barbershop);
                    }
                    else if (eventName is "PAYMENT_OVERDUE")
                    {
                        barbershop.SubscriptionStatus = SubscriptionStatusEnum.Inadimplente;
                        await barbershopService.UpdateEntityAsync(barbershop);
                    }
                    else if (eventName is "PAYMENT_REFUNDED" or "PAYMENT_DELETED")
                    {
                        barbershop.SubscriptionStatus = SubscriptionStatusEnum.Bloqueada;
                        await barbershopService.UpdateEntityAsync(barbershop);
                    }
                }

                return new(null, 200, "Webhook processado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao processar webhook - {ex.Message}");
            }
        }
    }
}



