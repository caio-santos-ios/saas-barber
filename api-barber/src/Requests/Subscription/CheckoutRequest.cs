using System;

namespace api_barber.Requests.Subscription
{
    public class CheckoutRequest
    {
        public string PlanId { get; set; } = string.Empty;
        public CreditCardRequest CreditCard { get; set; } = new();
    }

    public class CreditCardRequest
    {
        public string HolderName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string ExpiryMonth { get; set; } = string.Empty;
        public string ExpiryYear { get; set; } = string.Empty;
        public string Ccv { get; set; } = string.Empty;
    }
}
