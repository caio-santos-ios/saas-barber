namespace api_barber.Requests.Plan
{
    public class CreatePlanRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Level { get; set; }
    }
}
