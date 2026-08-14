using System.Text.Json;
using System.Threading.Tasks;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IWebhookService
    {
        Task<ResponseApi<object>> HandleAsaasWebhookAsync(JsonElement payload);
    }
}

