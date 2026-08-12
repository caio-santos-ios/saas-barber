using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using api_barber.Models.Enums;

namespace api_barber.Models
{
    public class Schedule : ModelBase
    {
        [BsonElement("day")]
        [BsonRepresentation(BsonType.String)]
        public DayOfWeekEnum Day { get; set; }

        [BsonElement("start_hour")]
        public TimeSpan StartHour { get; set; }

        [BsonElement("end_hour")]
        public TimeSpan EndHour { get; set; }

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
