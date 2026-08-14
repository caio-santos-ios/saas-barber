using api_barber.Models.Enums;
using System;
namespace api_barber.Requests.Schedule
{
    public class CreateScheduleRequest
    {
        public DayOfWeekEnum Day { get; set; }
        public TimeSpan StartHour { get; set; }
        public TimeSpan EndHour { get; set; }
        public int IntervalMinutes { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
    }
}

