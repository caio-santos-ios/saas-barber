namespace api_barber.Requests.Service
{
    public class CreateServiceRequest
    {
        public string ServiceTypeId { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public decimal? Commission { get; set; }
        public int? DurationMinutes { get; set; }
    }
}
