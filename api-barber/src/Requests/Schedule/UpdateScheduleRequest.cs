using api_barber.src.Requests;
using api_barber.Models.Enums;
using System;
namespace api_barber.Requests.Schedule
{
    public class UpdateScheduleRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public DayOfWeekEnum Day { get; set; }
        public string StartHour { get; set; } = string.Empty;
        public string EndHour { get; set; } = string.Empty;
        public string StartInterval { get; set; } = string.Empty;
        public string EndInterval { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
    }
}
