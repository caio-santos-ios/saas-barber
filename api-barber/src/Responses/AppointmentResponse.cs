using MongoDB.Bson.Serialization.Attributes;

namespace api_barber.src.Responses
{
    public class AppointmentResponse : Models.Appointment
    {
        [BsonElement("barber_name")]
        public string BarberName { get; set; } = string.Empty;

        [BsonElement("customer_name")]
        public string CustomerName { get; set; } = string.Empty;
    }
}
