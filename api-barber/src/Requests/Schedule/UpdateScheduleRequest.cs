using api_barber.Models.Enums;
using System;

namespace api_barber.Requests.Schedule
{
    public class UpdateScheduleRequest
    {
        public TimeSpan StartHour { get; set; }
        public TimeSpan EndHour { get; set; }
        public int IntervalMinutes { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}
