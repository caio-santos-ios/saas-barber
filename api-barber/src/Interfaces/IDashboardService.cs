using api_barber.Requests.Dashboard;
using api_barber.src.Requests;

namespace api_barber.Interfaces
{
    public interface IDashboardService
    {
        Task<ResponseApi<DashboardMetricsResponse>> GetMetricsAsync(string barbershopId, DashboardQueryRequest query);
    }
}
