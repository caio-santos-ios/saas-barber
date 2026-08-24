namespace api_barber.src.Requests
{
    public class RequestBase
    {
        public string CreatedBy { get; set; } = string.Empty;       
        public string UpdatedBy { get; set; } = string.Empty;       
        public string DeletedBy { get; set; } = string.Empty;       
        public string BarbershopId { get; set; } = string.Empty;       
    }
}