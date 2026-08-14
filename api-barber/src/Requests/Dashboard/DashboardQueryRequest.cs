using System;

namespace api_barber.Requests.Dashboard
{
    public class DashboardQueryRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
