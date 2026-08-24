using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetAllEntitiesAsync(string barbershopId);
        Task<ResponseApi<List<Appointment>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Appointment>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Appointment>> CreateAsync(Appointment entity);
        Task<ResponseApi<Appointment>> UpdateAsync(Appointment entity);
        Task<ResponseApi<Appointment>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
        Task<ResponseApi<List<Appointment>>> GetByBarberAndDateAsync(string barberId, DateTime date, string barbershopId);
    }
}


