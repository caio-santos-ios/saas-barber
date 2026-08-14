using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using api_barber.Models.Enums;
namespace api_barber.Models
{
    public class Notification : ModelBase
    {
        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public NotificationTypeEnum Type { get; set; }
        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;
        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;
        [BsonElement("user_id")]
        public string UserId { get; set; } = string.Empty;
        [BsonElement("target_role")]
        [BsonRepresentation(BsonType.String)]
        public TargetRoleEnum TargetRole { get; set; }
        [BsonElement("related_appointment_id")]
        public string RelatedAppointmentId { get; set; } = string.Empty;
        [BsonElement("read")]
        public bool Read { get; set; } = false;
        [BsonElement("barbershop_id")]
        public string BarbershopId { get; set; } = string.Empty;
    }
}

