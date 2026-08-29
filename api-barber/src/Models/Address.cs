using MongoDB.Bson.Serialization.Attributes;
namespace api_barber.Models
{
    public class Address
    {
        [BsonElement("street")]
        public string Street { get; set; } = string.Empty;
        [BsonElement("number")]
        public string Number { get; set; } = string.Empty;
        [BsonElement("complement")]
        public string Complement { get; set; } = string.Empty;
        [BsonElement("neighborhood")]
        public string Neighborhood { get; set; } = string.Empty;
        [BsonElement("city")]
        public string City { get; set; } = string.Empty;
        [BsonElement("state")]
        public string State { get; set; } = string.Empty;
        [BsonElement("zip_code")]
        public string ZipCode { get; set; } = string.Empty;
    }
}

