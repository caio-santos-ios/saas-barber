using api_barber.src.Requests;
namespace api_barber.Requests.Schedule
{
    public class CreateScheduleRequest : RequestBase
    {
        public string BarberId { get; set; } = string.Empty;
        public int Day { get; set; }
        public string StartHour { get; set; } = string.Empty;
        public string EndHour { get; set; } = string.Empty;
        public string? BreakStart { get; set; }
        public string? BreakEnd { get; set; }
        public int IntervalMinutes { get; set; } = 30;
        public string Notes { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
    }
}
