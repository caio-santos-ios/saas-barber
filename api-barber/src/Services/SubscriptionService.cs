using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.Subscription;
using api_barber.src.Requests;

namespace api_barber.Services
{
    public class SubscriptionService(IAsaasService asaasService, IBarbershopService barbershopService, IPlanService planService) : ISubscriptionService
    {
        public async Task<ResponseApi<object>> CheckoutAsync(CheckoutRequest request, string barbershopId)
        {
            if (string.IsNullOrEmpty(barbershopId)) return new(null, 400, "BarbershopId is required");

            ResponseApi<Barbershop> foundBarbershop = await barbershopService.GetByIdAsync(barbershopId);
            if (foundBarbershop.Data is null) return new(null, 404, "Barbershop not found");
            Barbershop barbershop = foundBarbershop.Data;

            var plans = await planService.GetAllAsync();
            var planDynamic = plans.Data?.FirstOrDefault(p => p.id == request.PlanId);
            if (planDynamic == null) return new(null, 404, "Plan not found");

            var planResponse = await planService.GetByIdAsync((string)planDynamic.id);
            if (planResponse.Data is null) return new(null, 404, "Plan not found");
            var plan = planResponse.Data;

            var billingType = request.BillingType?.ToUpper() switch
            {
                "PIX" => "PIX",
                "BOLETO" => "BOLETO",
                _ => "CREDIT_CARD"
            };

            if (billingType == "CREDIT_CARD" && request.CreditCard == null)
                return new(null, 400, "Dados do cartão de crédito são obrigatórios.");

            string asaasCustomerId = barbershop.AsaasCustomerId;
            if (string.IsNullOrEmpty(asaasCustomerId))
            {
                asaasCustomerId = await asaasService.CreateCustomerAsync(barbershop.Name, barbershop.Document, barbershop.Email);
                barbershop.AsaasCustomerId = asaasCustomerId;
                await barbershopService.UpdateEntityAsync(barbershop);
            }

            object? creditCardHolderInfo = null;
            if (billingType == "CREDIT_CARD")
            {
                creditCardHolderInfo = new
                {
                    name = string.IsNullOrEmpty(barbershop.Name) ? "Titular Padrão" : barbershop.Name,
                    email = string.IsNullOrEmpty(barbershop.Email) ? "titular@asaas.com" : barbershop.Email,
                    cpfCnpj = string.IsNullOrEmpty(barbershop.Document) ? "00000000000" : barbershop.Document,
                    postalCode = "01311000",
                    addressNumber = "123",
                    phone = string.IsNullOrEmpty(barbershop.Phone) ? "11999999999" : barbershop.Phone
                };
            }

            object paymentResult;
            try
            {
                paymentResult = await asaasService.CreateSubscriptionAsync(asaasCustomerId, plan.Id, plan.Price, billingType, request.CreditCard, creditCardHolderInfo);
            }
            catch (Exception ex)
            {
                return new(null, 400, $"Erro do Asaas: {ex.Message}");
            }

            barbershop.SubscriptionStatus = SubscriptionStatusEnum.Ativa;
            barbershop.PlanId = plan.Id;
            await barbershopService.UpdateEntityAsync(barbershop);

            return new(paymentResult, 200, "Assinatura ativada com sucesso");
        }

        public async Task<ResponseApi<object>> GetHistoryAsync(string barbershopId)
        {
            if (string.IsNullOrEmpty(barbershopId)) return new(null, 400, "BarbershopId is required");

            var barbershopResponse = await barbershopService.GetByIdAsync(barbershopId);
            if (barbershopResponse.Data is null || string.IsNullOrEmpty(barbershopResponse.Data.AsaasCustomerId))
                return new(null, 404, "Barbershop ou cliente Asaas não encontrado");

            var invoices = await asaasService.GetInvoicesAsync(barbershopResponse.Data.AsaasCustomerId);
            return new(invoices, 200, "Invoices retrieved successfully");
        }

        public async Task<ResponseApi<object>> CancelAsync(string barbershopId)
        {
            if (string.IsNullOrEmpty(barbershopId)) return new(null, 400, "BarbershopId is required");

            var barbershopResponse = await barbershopService.GetByIdAsync(barbershopId);
            if (barbershopResponse.Data is null)
                return new(null, 404, "Barbershop not found");

            var barbershop = barbershopResponse.Data;

            if (!string.IsNullOrEmpty(barbershop.AsaasCustomerId))
                await asaasService.CancelSubscriptionAsync(barbershop.AsaasCustomerId);

            barbershop.SubscriptionStatus = SubscriptionStatusEnum.Cancelada;
            await barbershopService.UpdateEntityAsync(barbershop);
            return new(null, 200, "Subscription cancelled successfully");
        }
    }
}


