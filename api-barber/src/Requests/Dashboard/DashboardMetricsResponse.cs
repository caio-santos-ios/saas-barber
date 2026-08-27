namespace api_barber.Requests.Dashboard
{
    public class DashboardMetricsResponse
    {
        public decimal TotalRevenue { get; set; }
        public int TotalAppointments { get; set; }
        public int ScheduledAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CanceledAppointments { get; set; }
        public int InProgressAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }

        public List<RankingItem> TopServices { get; set; } = [];
        public List<RankingItem> TopBarbers { get; set; } = [];

        public List<DailyRevenueItem> DailyRevenues { get; set; } = [];
        public List<HourlyItem> HourlyDistribution { get; set; } = [];
        public List<BarberRevenueItem> BarberRevenues { get; set; } = [];
    }

    public class RankingItem
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DailyRevenueItem
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Count { get; set; }
    }

    public class HourlyItem
    {
        public string Hour { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class BarberRevenueItem
    {
        public string Name { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Count { get; set; }
    }
}
