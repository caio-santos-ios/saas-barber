namespace api_barber.Requests.Auth
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string OriginUrl { get; set; } = string.Empty;
    }
}
