using api_barber.src.Requests;
namespace api_barber.Requests.Invoice
{
    public class UpdateInvoiceRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public string BarbershopId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
