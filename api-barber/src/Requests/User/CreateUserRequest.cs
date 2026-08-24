using api_barber.Models.Enums;
using api_barber.src.Requests;
namespace api_barber.Requests.User
{
    public class CreateUserRequest : RequestBase
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public RoleUserEnum Role { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Document { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
    }
}

