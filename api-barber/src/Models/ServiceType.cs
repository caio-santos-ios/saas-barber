using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace api_barber.Models
{
    public class ServiceType : ModelBase
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        [BsonElement("duration")]
        public int Duration { get; set; }
        [BsonElement("value")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Value { get; set; }
        [BsonElement("barbershop_id")]
        public string BarbershopId { get; set; } = string.Empty;
        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;
        [BsonElement("active")]
        public bool Active { get; set; } = true;
        [BsonElement("duration_minutes")]
        public int? DurationMinutes { get; set; }
    }
}

