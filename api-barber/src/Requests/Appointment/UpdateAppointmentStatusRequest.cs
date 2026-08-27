using api_barber.Models.Enums;
using api_barber.src.Requests;
namespace api_barber.Requests.Appointment
{
    public class UpdateAppointmentStatusRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public AppointmentStatusEnum Status { get; set; }
        public string CancelNotes { get; set; } = string.Empty;
    }
}

