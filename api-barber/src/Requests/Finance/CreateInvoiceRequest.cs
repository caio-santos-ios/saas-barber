using api_barber.Models.Enums;
namespace api_barber.Requests.Finance
{
    public class CreateInvoiceRequest
    {
        public DateTime DueDate { get; set; }
        public decimal Value { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}

