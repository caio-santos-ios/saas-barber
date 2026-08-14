using System;
namespace api_barber.Requests.Appointment
{
    public class CreateAppointmentRequest
    {
        public DateTime Date { get; set; }
        public string Hour { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceTypeId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ServiceTypeName { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}

