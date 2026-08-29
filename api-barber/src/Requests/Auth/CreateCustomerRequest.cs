namespace api_barber.Requests.Auth
{
    public class CreateCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BarbershopId { get; set; } = string.Empty;
        public string OriginUrl { get; set; } = string.Empty;
    }
}

