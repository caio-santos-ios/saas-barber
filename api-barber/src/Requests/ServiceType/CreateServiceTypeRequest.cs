using api_barber.src.Requests;
namespace api_barber.Requests.ServiceType
{
    public class CreateServiceTypeRequest : RequestBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; }
        public decimal Value { get; set; }
        public decimal? Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public int? DurationMinutes { get; set; }
    }
}
