using api_barber.src.Requests;
using api_barber.Models.Enums;

namespace api_barber.Requests.Appointment
{
    public class UpdateAppointmentRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Hour { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public string BarberName { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceTypeId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ServiceTypeName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public AppointmentStatusEnum Status { get; set; }
    }
}
