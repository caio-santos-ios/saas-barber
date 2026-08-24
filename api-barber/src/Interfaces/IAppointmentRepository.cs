using api_barber.Models;
using api_barber.src.Requests;
using MongoDB.Bson;

namespace api_barber.src.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<Appointment> GetByIdAsync(string id);
        Task<Appointment> CreateAsync(Appointment entity);
        Task<Appointment> UpdateAsync(Appointment entity);
        Task<Appointment> DeleteAsync(Appointment entity);
        
        Task<List<Appointment>> GetAllEntitiesAsync(string barbershopId);
        Task<List<Appointment>> GetByBarberAndDateAsync(string barberId, DateTime date, string barbershopId);
    }
}
