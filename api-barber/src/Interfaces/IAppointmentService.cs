using api_barber.Models;
using api_barber.Requests.Appointment;
using api_barber.src.Requests;

namespace api_barber.Interfaces
{
    public interface IAppointmentService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId, string? customerId = null, string? barberId = null);
        Task<ResponseApi<Appointment>> GetByIdAsync(string id);
        Task<ResponseApi<List<string>>> GetAvailableSlotsAsync(string barberId, DateTime date, string barbershopId);
        Task<ResponseApi<Appointment>> CreateAsync(CreateAppointmentRequest request);
        Task<ResponseApi<Appointment>> UpdateAsync(UpdateAppointmentRequest request);
        Task<ResponseApi<Appointment>> DeleteAsync(DeleteRequest request);
    }
}
