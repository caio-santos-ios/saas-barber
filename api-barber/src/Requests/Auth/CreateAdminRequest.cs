namespace api_barber.Requests.Auth
{
    public class CreateAdminRequest
    {
        public string BarbershopName { get; set; } = string.Empty;
        public string TypePerson { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
    }
}

