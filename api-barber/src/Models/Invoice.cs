using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using api_barber.Models.Enums;
namespace api_barber.Models
{
    public class Invoice : ModelBase
    {
        [BsonElement("date")]
        public DateTime Date { get; set; }
        [BsonElement("due_date")]
        public DateTime DueDate { get; set; }
        [BsonElement("paid_at")]
        public DateTime? PaidAt { get; set; }
        [BsonElement("value")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Value { get; set; }
        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public InvoiceStatusEnum Status { get; set; }
        [BsonElement("payment_method")]
        [BsonRepresentation(BsonType.String)]
        public PaymentMethodEnum PaymentMethod { get; set; }
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        [BsonElement("asaas_invoice_id")]
        public string AsaasInvoiceId { get; set; } = string.Empty;
        [BsonElement("asaas_customer_id")]
        public string AsaasCustomerId { get; set; } = string.Empty;
        [BsonElement("barbershop_id")]
        public string BarbershopId { get; set; } = string.Empty;
    }
}

