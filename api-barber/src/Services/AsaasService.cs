using api_barber.Interfaces;
using System.Text;
using System.Text.Json;

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
            var baseUrl = configuration["Asaas:BaseUrl"] ?? "https://api.asaas.com/v3/";
            _httpClient.BaseAddress = new System.Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("access_token", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SaaS-Barbearia/1.0");
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
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("ASAAS ERROR (Customer): " + errorBody);
            }
            return string.Empty;
        }
        public async Task<string> CreateSubscriptionAsync(string asaasCustomerId, string planId, decimal value, api_barber.Requests.Subscription.CreditCardRequest? creditCard = null, object? creditCardHolderInfo = null)
        {
            var payload = new
            {
                customer = asaasCustomerId,
                billingType = "CREDIT_CARD",
                value = value,
                nextDueDate = System.DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
                cycle = "MONTHLY",
                description = "Assinatura Plano " + planId,
                creditCard = creditCard != null ? new {
                    holderName = creditCard.HolderName,
                    number = creditCard.Number,
                    expiryMonth = creditCard.ExpiryMonth,
                    expiryYear = creditCard.ExpiryYear,
                    ccv = creditCard.Ccv
                } : null,
                creditCardHolderInfo = creditCardHolderInfo,
                remoteIp = "127.0.0.1"
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("subscriptions", content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                return json.GetProperty("id").GetString() ?? string.Empty;
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("ASAAS ERROR: " + errorBody);
                throw new Exception(errorBody);
            }
        }

        public async Task<object?> GetInvoicesAsync(string customerId)
        {
            var response = await _httpClient.GetAsync($"payments?customer={customerId}");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                if (json.TryGetProperty("data", out var data))
                {
                    return data;
                }
            }
            return null;
        }

        public async Task<bool> CancelSubscriptionAsync(string customerId)
        {
            var response = await _httpClient.GetAsync($"subscriptions?customer={customerId}&status=ACTIVE");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                if (json.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
                {
                    var subscriptionId = data[0].GetProperty("id").GetString();
                    var delResponse = await _httpClient.DeleteAsync($"subscriptions/{subscriptionId}");
                    return delResponse.IsSuccessStatusCode;
                }
            }
            return false;
        }
    }
}

