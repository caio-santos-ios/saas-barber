using api_barber.Requests.Subscription;
using api_barber.src.Requests;

namespace api_barber.Interfaces
{
    public interface ISubscriptionService
    {
        Task<ResponseApi<object>> CheckoutAsync(CheckoutRequest request, string barbershopId);
        Task<ResponseApi<object>> GetHistoryAsync(string barbershopId);
        Task<ResponseApi<object>> CancelAsync(string barbershopId);
    }
}
