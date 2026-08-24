using api_barber.src.Requests;
namespace api_barber.Requests.User
{
    public class UpdateUserRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Photo { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}

