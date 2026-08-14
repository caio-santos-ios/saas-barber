using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Requests.Appointment;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
using api_barber.Models.Enums;

namespace api_barber.Services
{
    public class AppointmentService(
        IAppointmentRepository repository,
        IScheduleRepository scheduleRepository,
        IUserRepository userRepository) : IAppointmentService
    {
        public async Task<ResponseApi<IEnumerable<string>>> GetAvailableSlotsAsync(string barberId, DateTime date, string barbershopId)
        {
            try
            {
                var schedulesResponse = await scheduleRepository.GetAllAsync(barbershopId);
                DayOfWeekEnum targetDay = (DayOfWeekEnum)(((int)date.DayOfWeek + 6) % 7);
                var schedule = schedulesResponse.Data?.FirstOrDefault(s => s.BarberId == barberId && s.Day == targetDay && s.Active);
                
                if (schedule == null) return new(new List<string>(), 200, "Nenhuma escala para este dia.");

                var appointmentsResponse = await repository.GetByBarberAndDateAsync(barberId, date, barbershopId);
                var existingAppointments = appointmentsResponse.Data?.Where(a => a.Status != AppointmentStatusEnum.Cancelado).ToList() ?? new List<Appointment>();

                var availableSlots = new List<string>();
                var currentTime = schedule.StartHour;

                while (currentTime < schedule.EndHour)
                {
                    string timeString = currentTime.ToString(@"hh\:mm");
                    if (!existingAppointments.Any(a => a.Hour == timeString))
                    {
                        availableSlots.Add(timeString);
                    }
                    currentTime = currentTime.Add(TimeSpan.FromMinutes(schedule.IntervalMinutes));
                }

                return new(availableSlots, 200, "Slots obtidos com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<Appointment>> CreateAsync(CreateAppointmentRequest request, string barbershopId)
        {
            try
            {
                var availabilityResponse = await GetAvailableSlotsAsync(request.BarberId, request.Date, barbershopId);
                if (availabilityResponse.Data == null || !availabilityResponse.Data.Contains(request.Hour))
                {
                    return new(null, 400, "Horário indisponível.");
                }

                Appointment appointment = ObjectMapper.Map<CreateAppointmentRequest, Appointment>(request);
                
                appointment.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                appointment.CreatedAt = DateTime.UtcNow;
                appointment.Status = AppointmentStatusEnum.Marcado;
                appointment.Deleted = false;
                appointment.BarbershopId = barbershopId;

                ResponseApi<Appointment> response = await repository.CreateAsync(appointment);
                return new(appointment, 201, "Agendamento feito com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new (null, 500, ex.Message + " " + ex.StackTrace);
            }
        }

        public async Task<ResponseApi<IEnumerable<Appointment>>> GetAllAsync(string barbershopId)
        {
            var response = await repository.GetAllAsync(barbershopId);
            if (response.Data == null) return response;

            var usersResponse = await userRepository.GetAllAsync(barbershopId);
            var users = usersResponse.Data?.ToList() ?? [];

            var enriched = response.Data.Select(apt =>
            {
                if (string.IsNullOrEmpty(apt.BarberName))
                    apt.BarberName = users.FirstOrDefault(u => u.Id == apt.BarberId)?.Name ?? "";
                if (string.IsNullOrEmpty(apt.CustomerName))
                    apt.CustomerName = users.FirstOrDefault(u => u.Id == apt.CustomerId)?.Name ?? "";
                return apt;
            });

            return new(enriched, 200, "Listagem obtida com sucesso");
        }

        public async Task<ResponseApi<Appointment>> GetByIdAsync(string id, string barbershopId)
        {
            return await repository.GetByIdAsync(id, barbershopId);
        }

        public async Task<ResponseApi<Appointment>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            try
            {
                var existingResponse = await repository.GetByIdAsync(id, barbershopId);
                if (existingResponse.Data == null) return new(null, 404, "Agendamento não encontrado");

                Appointment entity = existingResponse.Data;
                
                entity.Deleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = deletedBy;
                entity.Status = AppointmentStatusEnum.Cancelado;

                var updateResponse = await repository.UpdateAsync(entity);
                if (updateResponse.Status == 200)
                {
                     return new(null, 200, "Agendamento excluído com sucesso");
                }
                return updateResponse;
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<Appointment>> UpdateAsync(UpdateAppointmentStatusRequest request)
        {
            try
            {
                var existingResponse = await repository.GetByIdAsync(request.Id, string.Empty);
                if (existingResponse.Data == null) return new(null, 404, "Registro não encontrado");

                Appointment entity = existingResponse.Data;
                
                entity.Status = request.Status;
                entity.UpdatedAt = DateTime.UtcNow;
                
                if (request.Status == AppointmentStatusEnum.Cancelado)
                {
                    entity.CancelNotes = request.CancelNotes;
                }

                return await repository.UpdateAsync(entity);
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
    }
}
