using MongoDB.Driver;
using api_barber.Models;
using System.Collections.Generic;
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
        
        
        
        api_barber.Infrastructures.AppDbContext appDbContext) : IDashboardService
    {
        public async Task<ResponseApi<DashboardMetricsResponse>> GetMetricsAsync(string barbershopId, DashboardQueryRequest query)
        {
            try
            {
                var appointmentsResponse = await MongoDB.Driver.IAsyncCursorSourceExtensions.ToListAsync(appDbContext.Appointments.Find(x => !x.Deleted && x.BarbershopId == barbershopId));
                var servicesResponse = await MongoDB.Driver.IAsyncCursorSourceExtensions.ToListAsync(appDbContext.Services.Find(x => !x.Deleted && x.BarbershopId == barbershopId));
                var serviceTypesResponse = await MongoDB.Driver.IAsyncCursorSourceExtensions.ToListAsync(appDbContext.ServiceTypes.Find(x => !x.Deleted && x.BarbershopId == barbershopId));
                var usersResponse = await MongoDB.Driver.IAsyncCursorSourceExtensions.ToListAsync(appDbContext.Users.Find(x => !x.Deleted && x.BarbershopId == barbershopId));

                var allAppointments = appointmentsResponse ?? new List<Appointment>();
                var allServices = servicesResponse ?? new List<Service>();
                var allServiceTypes = serviceTypesResponse ?? new List<ServiceType>();
                var allUsers = usersResponse ?? new List<User>();

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





