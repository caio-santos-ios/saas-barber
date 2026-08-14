using System.Linq;
using System.Threading.Tasks;
using api_barber.Interfaces;
using api_barber.Requests.Dashboard;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.Models.Enums;

namespace api_barber.Services
{
    public class DashboardService(
        IAppointmentRepository appointmentRepository,
        IServiceRepository serviceRepository,
        IServiceTypeRepository serviceTypeRepository,
        IUserRepository userRepository) : IDashboardService
    {
        public async Task<ResponseApi<DashboardMetricsResponse>> GetMetricsAsync(string barbershopId, DashboardQueryRequest query)
        {
            try
            {
                var appointmentsResponse = await appointmentRepository.GetAllAsync(barbershopId);
                var servicesResponse = await serviceRepository.GetAllAsync(barbershopId);
                var serviceTypesResponse = await serviceTypeRepository.GetAllAsync(barbershopId);
                var usersResponse = await userRepository.GetAllAsync(barbershopId);

                var allAppointments = appointmentsResponse.Data ?? [];
                var allServices = servicesResponse.Data ?? [];
                var allServiceTypes = serviceTypesResponse.Data ?? [];
                var allUsers = usersResponse.Data ?? [];

                var periodAppointments = allAppointments.Where(a => a.Date >= query.StartDate && a.Date <= query.EndDate).ToList();

                var metrics = new DashboardMetricsResponse
                {
                    TotalAppointments = periodAppointments.Count,
                    CompletedAppointments = periodAppointments.Count(a => a.Status == AppointmentStatusEnum.Finalizado),
                    CanceledAppointments = periodAppointments.Count(a => a.Status == AppointmentStatusEnum.Cancelado),
                    InProgressAppointments = 0, 
                    ConfirmedAppointments = periodAppointments.Count(a => a.Status == AppointmentStatusEnum.Marcado)
                };

                var completedApps = periodAppointments.Where(a => a.Status == AppointmentStatusEnum.Finalizado);
                metrics.TotalRevenue = completedApps.Sum(a => a.Value);

                metrics.TopServices = periodAppointments
                    .GroupBy(a => a.ServiceId)
                    .Select(g => {
                        var serviceType = allServiceTypes.FirstOrDefault(st => st.Id == g.Key);
                        var fallbackName = g.FirstOrDefault()?.ServiceTypeName;
                        return new RankingItem
                        {
                            Name = serviceType?.Name ?? (!string.IsNullOrEmpty(fallbackName) ? fallbackName : "Desconhecido"),
                            Count = g.Count()
                        };
                    })
                    .OrderByDescending(r => r.Count)
                    .Take(5)
                    .ToList();

                metrics.TopBarbers = periodAppointments
                    .GroupBy(a => a.BarberId)
                    .Select(g => new RankingItem
                    {
                        Name = allUsers.FirstOrDefault(u => u.Id == g.Key)?.Name ?? "Desconhecido",
                        Count = g.Count()
                    })
                    .OrderByDescending(r => r.Count)
                    .Take(5)
                    .ToList();

                return new(metrics, 200, "Métricas obtidas com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
    }
}
