namespace api_barber.Interfaces
{
    public interface IAsaasService
    {
        Task<string> CreateCustomerAsync(string name, string document, string email);
        Task<object> CreateSubscriptionAsync(string asaasCustomerId, string planId, decimal value, string billingType, api_barber.Requests.Subscription.CreditCardRequest? creditCard = null, object? creditCardHolderInfo = null);
        Task<object?> GetInvoicesAsync(string customerId);
        Task<bool> CancelSubscriptionAsync(string customerId);
    }
}

