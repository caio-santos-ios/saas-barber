using System.Threading.Tasks;

namespace api_barber.Interfaces
{
    public interface IAsaasService
    {
        Task<string> CreateCustomerAsync(string name, string document, string email);
        Task<string> CreateSubscriptionAsync(string asaasCustomerId, string planId, decimal value);
    }
}
