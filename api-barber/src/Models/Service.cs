using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_barber.Models
{
    public class Service : ModelBase
    {
        [BsonElement("service_type_id")]
        public string ServiceTypeId { get; set; } = string.Empty;

        [BsonElement("barber_id")]
        public string BarberId { get; set; } = string.Empty;

        [BsonElement("barbershop_id")]
        public string BarbershopId { get; set; } = string.Empty;

        [BsonElement("active")]
        public bool Active { get; set; } = true;

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal? Price { get; set; }

        [BsonElement("commission")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal? Commission { get; set; }

        [BsonElement("duration_minutes")]
        public int? DurationMinutes { get; set; }
    }
}
