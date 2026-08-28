using api_barber.src.Requests;
namespace api_barber.Requests.Notification
{
    public class CreateNotificationRequest : RequestBase
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Read { get; set; } = false;
        public bool Send { get; set; } = false;
        public DateTime SendAt { get; set; }
    }
}
