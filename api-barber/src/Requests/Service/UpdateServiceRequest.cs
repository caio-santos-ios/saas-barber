namespace api_barber.Requests.Service
{
    public class UpdateServiceRequest
    {
        public decimal? Price { get; set; }
        public decimal? Commission { get; set; }
        public int? DurationMinutes { get; set; }
        public bool Active { get; set; }
    }
}
