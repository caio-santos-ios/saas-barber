using api_barber.Models.Enums;
namespace api_barber.Requests.Appointment
{
    public class UpdateAppointmentStatusRequest
    {
        public string Id { get; set; } = string.Empty;
        public AppointmentStatusEnum Status { get; set; }
        public string CancelNotes { get; set; } = string.Empty;
    }
}

