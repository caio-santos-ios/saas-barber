namespace api_barber.Requests.Barbershop
{
    public class UpdateBarbershopRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}

