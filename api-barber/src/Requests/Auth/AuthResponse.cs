namespace api_barber.Requests.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BarbershopId { get; set; } = string.Empty;
    }
}
