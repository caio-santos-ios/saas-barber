using api_barber.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace api_barber.Services
{
    public class AsaasService : IAsaasService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AsaasService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Asaas:ApiKey"] ?? string.Empty;
            _httpClient.BaseAddress = new System.Uri("https://sandbox.asaas.com/api/v3/");
            _httpClient.DefaultRequestHeaders.Add("access_token", _apiKey);
        }

        public async Task<string> CreateCustomerAsync(string name, string document, string email)
        {
            var payload = new
            {
                name = name,
                cpfCnpj = document,
                email = email
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("customers", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                return json.GetProperty("id").GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        public async Task<string> CreateSubscriptionAsync(string asaasCustomerId, string planId, decimal value)
        {
            var payload = new
            {
                customer = asaasCustomerId,
                billingType = "CREDIT_CARD",
                value = value,
                nextDueDate = System.DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
                cycle = "MONTHLY",
                description = "Assinatura Plano " + planId
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("subscriptions", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                return json.GetProperty("id").GetString() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
