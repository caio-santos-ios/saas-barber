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
            _apiKey = (Environment.GetEnvironmentVariable("ASAAS_API_KEY") ?? configuration["Asaas:ApiKey"] ?? string.Empty).Trim();
            var baseUrl = Environment.GetEnvironmentVariable("ASAAS_BASE_URL") ?? configuration["Asaas:BaseUrl"] ?? "https://api.asaas.com/v3/";
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            _httpClient.BaseAddress = new System.Uri(baseUrl);
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("access_token", _apiKey);
            }
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SaaS-Barbearia/1.0");
        }

        public async Task<string> CreateCustomerAsync(string name, string document, string email)
        {
            var payload = new { name, cpfCnpj = document, email };
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
                try
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(errorBody);
                    var errorMessage = json.GetProperty("errors")[0].GetProperty("description").GetString();
                    throw new Exception(errorMessage);
                }
                catch (Exception ex) when (ex is not JsonException && ex is not KeyNotFoundException)
                {
                    throw;
                }
                throw new Exception("Erro ao cadastrar cliente no Asaas.");
            }
        }

        public async Task<object> CreateSubscriptionAsync(string asaasCustomerId, string planId, decimal value, string billingType, api_barber.Requests.Subscription.CreditCardRequest? creditCard = null, object? creditCardHolderInfo = null)
        {
            object payload;

            if (billingType == "CREDIT_CARD" && creditCard != null)
            {
                payload = new
                {
                    customer = asaasCustomerId,
                    billingType = "CREDIT_CARD",
                    value,
                    nextDueDate = System.DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                    cycle = "MONTHLY",
                    description = "Assinatura Plano " + planId,
                    creditCard = new
                    {
                        holderName = creditCard.HolderName,
                        number = creditCard.Number,
                        expiryMonth = creditCard.ExpiryMonth,
                        expiryYear = creditCard.ExpiryYear,
                        ccv = creditCard.Ccv
                    },
                    creditCardHolderInfo,
                    remoteIp = "127.0.0.1"
                };
            }
            else
            {
                payload = new
                {
                    customer = asaasCustomerId,
                    billingType,
                    value,
                    nextDueDate = System.DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                    cycle = "MONTHLY",
                    description = "Assinatura Plano " + planId
                };
            }

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("subscriptions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                try
                {
                    var errJson = JsonSerializer.Deserialize<JsonElement>(errorBody);
                    var errMsg = errJson.GetProperty("errors")[0].GetProperty("description").GetString();
                    throw new Exception(errMsg);
                }
                catch (Exception ex) when (ex is not JsonException && ex is not KeyNotFoundException)
                {
                    throw;
                }
                throw new Exception("Erro ao criar assinatura no Asaas.");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var subscriptionId = json.GetProperty("id").GetString() ?? string.Empty;

            if (billingType == "PIX")
            {
                await System.Threading.Tasks.Task.Delay(1500);
                var paymentRes = await _httpClient.GetAsync($"payments?subscription={subscriptionId}");
                if (paymentRes.IsSuccessStatusCode)
                {
                    var paymentBody = await paymentRes.Content.ReadAsStringAsync();
                    var paymentJson = JsonSerializer.Deserialize<JsonElement>(paymentBody);
                    var payments = paymentJson.GetProperty("data");
                    if (payments.GetArrayLength() > 0)
                    {
                        var paymentId = payments[0].GetProperty("id").GetString();
                        var pixRes = await _httpClient.GetAsync($"payments/{paymentId}/pixQrCode");
                        if (pixRes.IsSuccessStatusCode)
                        {
                            var pixBody = await pixRes.Content.ReadAsStringAsync();
                            var pixJson = JsonSerializer.Deserialize<JsonElement>(pixBody);
                            return new
                            {
                                subscriptionId,
                                paymentMethod = "PIX",
                                pixQrCode = pixJson.TryGetProperty("encodedImage", out var img) ? img.GetString() : null,
                                pixKey = pixJson.TryGetProperty("payload", out var key) ? key.GetString() : null,
                                expirationDate = pixJson.TryGetProperty("expirationDate", out var exp) ? exp.GetString() : null
                            };
                        }
                    }
                }
                return new { subscriptionId, paymentMethod = "PIX" };
            }

            if (billingType == "BOLETO")
            {
                await System.Threading.Tasks.Task.Delay(1500);
                var paymentRes = await _httpClient.GetAsync($"payments?subscription={subscriptionId}");
                if (paymentRes.IsSuccessStatusCode)
                {
                    var paymentBody = await paymentRes.Content.ReadAsStringAsync();
                    var paymentJson = JsonSerializer.Deserialize<JsonElement>(paymentBody);
                    var payments = paymentJson.GetProperty("data");
                    if (payments.GetArrayLength() > 0)
                    {
                        var payment = payments[0];
                        return new
                        {
                            subscriptionId,
                            paymentMethod = "BOLETO",
                            boletoUrl = payment.TryGetProperty("bankSlipUrl", out var burl) ? burl.GetString() : null,
                            boletoBarCode = payment.TryGetProperty("nossoNumero", out var nn) ? nn.GetString() : null,
                            dueDate = payment.TryGetProperty("dueDate", out var dd) ? dd.GetString() : null
                        };
                    }
                }
                return new { subscriptionId, paymentMethod = "BOLETO" };
            }

            return new { subscriptionId, paymentMethod = "CREDIT_CARD" };
        }

        public async Task<object?> GetInvoicesAsync(string customerId)
        {
            var response = await _httpClient.GetAsync($"payments?customer={customerId}");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                if (json.TryGetProperty("data", out var data))
                    return data;
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
