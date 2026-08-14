using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<ResponseApi<IEnumerable<Appointment>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Appointment>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Appointment>> CreateAsync(Appointment entity);
        Task<ResponseApi<Appointment>> UpdateAsync(Appointment entity);
        Task<ResponseApi<Appointment>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
        Task<ResponseApi<IEnumerable<Appointment>>> GetByBarberAndDateAsync(string barberId, DateTime date, string barbershopId);
    }
}

