using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_barber.Models
{
    public class Plan : ModelBase
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("asaas_plan_id")]
        public string AsaasPlanId { get; set; } = string.Empty;

        [BsonElement("level")]
        public int Level { get; set; }

        [BsonElement("active")]
        public bool Active { get; set; } = true;
    }
}
