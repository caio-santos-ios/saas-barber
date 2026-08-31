using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using api_barber.Models.Enums;
namespace api_barber.Models
{
    public class Appointment : ModelBase
    {
        [BsonElement("date")]
        public DateTime Date { get; set; }
        [BsonElement("hour")]
        public string Hour { get; set; } = string.Empty;
        [BsonElement("notes")]
        public string Notes { get; set; } = string.Empty;
        [BsonElement("cancel_notes")]
        public string CancelNotes { get; set; } = string.Empty;
        [BsonElement("status")]
        public AppointmentStatusEnum Status { get; set; }
        [BsonElement("barber_id")]
        public string BarberId { get; set; } = string.Empty;

        [BsonElement("customer_id")]
        public string CustomerId { get; set; } = string.Empty;
        [BsonElement("service_id")]
        public string ServiceId { get; set; } = string.Empty;
        [BsonElement("service_type_id")]
        public string ServiceTypeId { get; set; } = string.Empty;
        [BsonElement("barbershop_id")]
        public string BarbershopId { get; set; } = string.Empty;
        [BsonElement("value")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Value { get; set; }
        [BsonElement("payment_status")]
        public string PaymentStatus { get; set; } = string.Empty;
        [BsonElement("asaas_payment_id")]
        public string AsaasPaymentId { get; set; } = string.Empty;
        [BsonElement("started_at")]
        public DateTime? StartedAt { get; set; }
        [BsonElement("finished_at")]
        public DateTime? FinishedAt { get; set; }
        [BsonElement("rating")]
        public int? Rating { get; set; }
        [BsonElement("rating_comment")]
        public string? RatingComment { get; set; }
    }
}

