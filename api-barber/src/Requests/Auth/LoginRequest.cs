using api_barber.Models.Enums;

namespace api_barber.Requests.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? TokenFCM { get; set; }
        public RoleUserEnum? Role { get; set; }
        public string BarbershopId { get; set; } = string.Empty;
    }
}


