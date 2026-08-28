using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace api_barber.Models
{
    public class Schedule : ModelBase
    {
        [BsonElement("day")]
        public int Day { get; set; }
        [BsonElement("start_hour")]
        public TimeSpan StartHour { get; set; }
        [BsonElement("end_hour")]
        public TimeSpan EndHour { get; set; }
        [BsonElement("break_start")]
        public TimeSpan? BreakStart { get; set; }
        [BsonElement("break_end")]
        public TimeSpan? BreakEnd { get; set; }
        [BsonElement("interval_minutes")]
        public int IntervalMinutes { get; set; }
        [BsonElement("notes")]
        public string Notes { get; set; } = string.Empty;
        [BsonElement("barber_id")]
        public string BarberId { get; set; } = string.Empty;
        [BsonElement("barbershop_id")]
        public string BarbershopId { get; set; } = string.Empty;
        [BsonElement("active")]
        public bool Active { get; set; } = true;
    }
}
