using api_barber.src.Requests;
namespace api_barber.Requests.Service
{
    public class UpdateServiceRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; }
        public decimal Value { get; set; }
        public string ServiceTypeId { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
    }
}
