using api_barber.src.Requests;
namespace api_barber.Requests.Schedule
{
    public class UpdateScheduleRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public int Day { get; set; }
        public string StartHour { get; set; } = string.Empty;
        public string EndHour { get; set; } = string.Empty;
        public int IntervalMinutes { get; set; } = 30;
        public string Notes { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
    }
}
