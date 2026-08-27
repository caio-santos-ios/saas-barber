using api_barber.Models;
using api_barber.src.Requests;

namespace api_barber.Requests.Barbershop
{
    public class UpdateBarbershopRequest : RequestBase
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public Address Address { get; set; } = new Address();
        public string Code { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string AsaasCustomerId { get; set; } = string.Empty;
    }
}
