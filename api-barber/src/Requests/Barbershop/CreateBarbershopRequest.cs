using api_barber.src.Requests;
using api_barber.Models.Enums;
using System;
namespace api_barber.Requests.Barbershop
{
    public class CreateBarbershopRequest : RequestBase
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Complement { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string StripeCustomerId { get; set; } = string.Empty;
        public string StripeSubscriptionId { get; set; } = string.Empty;
        public string AsaasCustomerId { get; set; } = string.Empty;
        public string AsaasSubscriptionId { get; set; } = string.Empty;
    }
}
