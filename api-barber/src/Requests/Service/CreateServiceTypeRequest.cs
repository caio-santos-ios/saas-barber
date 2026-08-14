namespace api_barber.Requests.Service
{
    public class CreateServiceTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Value { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}

