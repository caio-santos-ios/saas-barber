namespace api_barber.Requests.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BarbershopId { get; set; } = string.Empty;
        public string SubscriptionStatus { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
    }

    public class ResetPasswordResponse
    {
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}

