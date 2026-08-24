using api_barber.src.Requests;
using api_barber.Models.Enums;
using System;
namespace api_barber.Requests.Plan
{
    public class CreatePlanRequest : RequestBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public bool Active { get; set; } = true;
    }
}
