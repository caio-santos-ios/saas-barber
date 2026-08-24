using api_barber.src.Requests;
namespace api_barber.Requests.Notification
{
    public class UpdateNotificationRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Read { get; set; } = true;
    }
}
