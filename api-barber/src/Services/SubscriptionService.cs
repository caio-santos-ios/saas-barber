using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.Subscription;
using api_barber.src.Requests;
using System.Linq;
using System.Threading.Tasks;

namespace api_barber.Services
{
    public class SubscriptionService(IAsaasService asaasService, IBarbershopService barbershopService, IPlanService planService) : ISubscriptionService
    {
        public async Task<ResponseApi<object>> CheckoutAsync(CheckoutRequest request, string barbershopId)
        {
            if (string.IsNullOrEmpty(barbershopId)) return new(null, 400, "BarbershopId is required");
            
            var barbershops = await barbershopService.GetAllAsync(string.Empty);
            var barbershop = barbershops.Data?.FirstOrDefault(b => b.Id == barbershopId);
            if (barbershop == null) return new(null, 404, "Barbershop not found");

            var plans = await planService.GetAllAsync(string.Empty);
            var plan = plans.Data?.FirstOrDefault(p => p.Id == request.PlanId);
            if (plan == null) return new(null, 404, "Plan not found");

            string asaasCustomerId = barbershop.AsaasCustomerId;
            if (string.IsNullOrEmpty(asaasCustomerId))
            {
                asaasCustomerId = await asaasService.CreateCustomerAsync(barbershop.Name, barbershop.Document, barbershop.Email);
                barbershop.AsaasCustomerId = asaasCustomerId;
                await barbershopService.UpdateAsync(barbershop.Id, barbershop, string.Empty);
            }

            var creditCardHolderInfo = new {
                name = barbershop.Name,
                email = barbershop.Email,
                cpfCnpj = string.IsNullOrEmpty(barbershop.Document) ? "00000000000" : barbershop.Document,
                postalCode = "01311000",
                addressNumber = "123",
                phone = string.IsNullOrEmpty(barbershop.Phone) ? "11999999999" : barbershop.Phone
            };

            var subscriptionId = await asaasService.CreateSubscriptionAsync(asaasCustomerId, plan.Id, plan.Price, request.CreditCard, creditCardHolderInfo);
            
            if (string.IsNullOrEmpty(subscriptionId))
            {
                return new(null, 400, "Failed to process payment in Asaas. Check credit card details.");
            }

            barbershop.SubscriptionStatus = SubscriptionStatusEnum.Ativa;
            barbershop.PlanId = plan.Id;
            await barbershopService.UpdateAsync(barbershop.Id, barbershop, string.Empty);

            return new(subscriptionId, 200, "Subscription activated successfully");
        }

        public async Task<ResponseApi<object>> GetHistoryAsync(string barbershopId)
        {
            if (string.IsNullOrEmpty(barbershopId)) return new(null, 400, "BarbershopId is required");
            var barbershops = await barbershopService.GetAllAsync(string.Empty);
            var barbershop = barbershops.Data?.FirstOrDefault(b => b.Id == barbershopId);
            if (barbershop == null || string.IsNullOrEmpty(barbershop.AsaasCustomerId)) 
                return new(null, 404, "Barbershop or Asaas Customer not found");

            var invoices = await asaasService.GetInvoicesAsync(barbershop.AsaasCustomerId);
            return new(invoices, 200, "Invoices retrieved successfully");
        }

        public async Task<ResponseApi<object>> CancelAsync(string barbershopId)
        {
            if (string.IsNullOrEmpty(barbershopId)) return new(null, 400, "BarbershopId is required");
            var barbershops = await barbershopService.GetAllAsync(string.Empty);
            var barbershop = barbershops.Data?.FirstOrDefault(b => b.Id == barbershopId);
            if (barbershop == null) 
                return new(null, 404, "Barbershop not found");

            if (!string.IsNullOrEmpty(barbershop.AsaasCustomerId))
            {
                var success = await asaasService.CancelSubscriptionAsync(barbershop.AsaasCustomerId);
                // Mesmo se falhar na Asaas (ex: não há assinatura ativa lá), prosseguimos para cancelar localmente
            }

            barbershop.SubscriptionStatus = SubscriptionStatusEnum.Cancelada;
            await barbershopService.UpdateAsync(barbershop.Id, barbershop, string.Empty);
            return new(null, 200, "Subscription cancelled successfully");
        }
    }
}
