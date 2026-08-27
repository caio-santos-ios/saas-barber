using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using api_barber.Models.Enums;
namespace api_barber.Models
{
    public class Barbershop : ModelBase
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        [BsonElement("type_person")]
        [BsonRepresentation(BsonType.String)]
        public TypePersonEnum TypePerson { get; set; }
        [BsonElement("document")]
        public string Document { get; set; } = string.Empty;
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;
        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;
        [BsonElement("whatsapp")]
        public string WhatsApp { get; set; } = string.Empty;
        [BsonElement("address")]
        public Address Address { get; set; } = new Address();
        [BsonElement("logo")]
        public string Logo { get; set; } = string.Empty;
        [BsonElement("plan_id")]
        public string PlanId { get; set; } = string.Empty;
        [BsonElement("asaas_customer_id")]
        public string AsaasCustomerId { get; set; } = string.Empty;
        [BsonElement("subscription_status")]
        [BsonRepresentation(BsonType.String)]
        public SubscriptionStatusEnum SubscriptionStatus { get; set; }
        [BsonElement("code")]
        public string Code { get; set; } = string.Empty;
        [BsonElement("active")]
        public bool Active { get; set; } = true;
    }
}

