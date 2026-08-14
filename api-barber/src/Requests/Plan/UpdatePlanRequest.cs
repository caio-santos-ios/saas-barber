namespace api_barber.Requests.Plan
{
    public class UpdatePlanRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Level { get; set; }
        public bool Active { get; set; }
    }
}

