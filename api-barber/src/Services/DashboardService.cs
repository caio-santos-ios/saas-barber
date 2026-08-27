using MongoDB.Driver;
using api_barber.Models;
using api_barber.Interfaces;
using api_barber.Requests.Dashboard;
using api_barber.src.Requests;
using api_barber.Models.Enums;
using api_barber.Infrastructures;

namespace api_barber.Services
{
    public class DashboardService(AppDbContext appDbContext) : IDashboardService
    {
        public async Task<ResponseApi<DashboardMetricsResponse>> GetMetricsAsync(string barbershopId, DashboardQueryRequest query)
        {
            try
            {
                var cleanBarbershopId = (barbershopId ?? "").Trim();
                var appointmentsResponse = await appDbContext.Appointments.Find(x => x.Deleted != true && x.BarbershopId == cleanBarbershopId).ToListAsync();
                var servicesResponse = await appDbContext.Services.Find(x => x.Deleted != true && x.BarbershopId == cleanBarbershopId).ToListAsync();
                var serviceTypesResponse = await appDbContext.ServiceTypes.Find(x => x.Deleted != true && x.BarbershopId == cleanBarbershopId).ToListAsync();
                var usersResponse = await appDbContext.Users.Find(x => x.Deleted != true && x.BarbershopId == cleanBarbershopId).ToListAsync();

                var allAppointments = appointmentsResponse ?? new List<Appointment>();
                var allServices = servicesResponse ?? new List<Service>();
                var allServiceTypes = serviceTypesResponse ?? new List<ServiceType>();
                var allUsers = usersResponse ?? new List<User>();

                var startOfPeriod = query.StartDate == default ? DateTime.MinValue : query.StartDate.Date;
                var endOfPeriod = query.EndDate == default ? DateTime.MaxValue : query.EndDate.Date.AddDays(1).AddTicks(-1);

                var periodAppointments = allAppointments.Where(a => a.Date.Date >= startOfPeriod.Date && a.Date.Date <= endOfPeriod.Date).ToList();

                var metrics = new DashboardMetricsResponse
                {
                    TotalAppointments = periodAppointments.Count,
                    CompletedAppointments = periodAppointments.Count(a => a.Status == AppointmentStatusEnum.Finalizado || (int)a.Status == 3),
                    CanceledAppointments = periodAppointments.Count(a => a.Status == AppointmentStatusEnum.Cancelado || (int)a.Status == 2),
                    InProgressAppointments = 0,
                    ConfirmedAppointments = periodAppointments.Count(a => a.Status == AppointmentStatusEnum.Marcado || (int)a.Status == 1 || (int)a.Status == 0)
                };

                var completedApps = periodAppointments.Where(a => a.Status == AppointmentStatusEnum.Finalizado || (int)a.Status == 3);
                metrics.TotalRevenue = completedApps.Sum(a => a.Value);

                var serviceTypeMap = allServiceTypes.ToDictionary(st => st.Id, st => st.Name);
                var userMap = allUsers.ToDictionary(u => u.Id, u => u.Name);

                metrics.TopServices = periodAppointments
                    .Where(a => !string.IsNullOrEmpty(a.ServiceTypeId))
                    .GroupBy(a => a.ServiceTypeId)
                    .Select(g => new RankingItem
                    {
                        Name = serviceTypeMap.GetValueOrDefault(g.Key) ?? "Serviço",
                        Count = g.Count()
                    })
                    .OrderByDescending(r => r.Count)
                    .Take(5)
                    .ToList();

                metrics.TopBarbers = periodAppointments
                    .Where(a => !string.IsNullOrEmpty(a.BarberId))
                    .GroupBy(a => a.BarberId)
                    .Select(g => new RankingItem
                    {
                        Name = userMap.GetValueOrDefault(g.Key) ?? "Profissional",
                        Count = g.Count()
                    })
                    .OrderByDescending(r => r.Count)
                    .Take(5)
                    .ToList();

                return new(metrics, 200, "Métricas obtidas com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
    }
}

