namespace api_barber.Requests.Auth
{
    public class ConfirmResetPasswordRequest
    {
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
