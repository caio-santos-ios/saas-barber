using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using api_barber.Models.Enums;
namespace api_barber.Models
{
    [BsonIgnoreExtraElements]
    public class User : ModelBase
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;
        [BsonElement("whatsapp")]
        public string WhatsApp { get; set; } = string.Empty;
        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)]
        public RoleUserEnum Role { get; set; }
        [BsonElement("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }
        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;
        [BsonElement("token_fcm")]
        public string? TokenFCM { get; set; }
        [BsonElement("firebase_uid")]
        public string FirebaseUid { get; set; } = string.Empty;
        [BsonElement("photo")]
        public string Photo { get; set; } = string.Empty;
        [BsonElement("document")]
        public string Document { get; set; } = string.Empty;
        [BsonElement("barbershop_id")]
        public string BarbershopId { get; set; } = string.Empty;
        [BsonElement("password_reset_required")]
        public bool PasswordResetRequired { get; set; } = false;
        [BsonElement("email_confirmed")]
        public bool EmailConfirmed { get; set; } = false;
        [BsonElement("active")]
        public bool Active { get; set; } = true;
    }
}


