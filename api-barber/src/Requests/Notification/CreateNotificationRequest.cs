using api_barber.Models.Enums;
namespace api_barber.Requests.Notification
{
    public class CreateNotificationRequest
    {
        public NotificationTypeEnum Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public TargetRoleEnum TargetRole { get; set; }
        public string RelatedAppointmentId { get; set; } = string.Empty;
    }
}

