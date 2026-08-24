using api_barber.Models;
using api_barber.Requests.Appointment;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IAppointmentService
    {
        Task<ResponseApi<List<Appointment>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Appointment>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Appointment>> CreateEntityAsync(Appointment entity);
        Task<ResponseApi<Appointment>> CreateAsync(CreateAppointmentRequest request, string barbershopId);
        Task<ResponseApi<Appointment>> UpdateAsync(UpdateAppointmentStatusRequest request);
        Task<ResponseApi<Appointment>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
        Task<ResponseApi<List<string>>> GetAvailableSlotsAsync(string barberId, DateTime date, string barbershopId);
    }
}

