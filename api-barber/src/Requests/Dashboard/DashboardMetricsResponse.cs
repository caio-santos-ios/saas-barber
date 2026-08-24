using System.Collections.Generic;

namespace api_barber.Requests.Dashboard
{
    public class DashboardMetricsResponse
    {
        public decimal TotalRevenue { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CanceledAppointments { get; set; }
        public int InProgressAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }

        public List<RankingItem> TopServices { get; set; } = [];
        public List<RankingItem> TopBarbers { get; set; } = [];
    }

    public class RankingItem
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
