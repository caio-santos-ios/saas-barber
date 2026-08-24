using api_barber.src.Requests;
using api_barber.Models.Enums;
namespace api_barber.Requests.Schedule
{
    public class CreateScheduleRequest : RequestBase
    {
        public string BarberId { get; set; } = string.Empty;
        public DayOfWeekEnum Day { get; set; }
        public string StartHour { get; set; } = string.Empty;
        public string EndHour { get; set; } = string.Empty;
        public string StartInterval { get; set; } = string.Empty;
        public string EndInterval { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
    }
}
