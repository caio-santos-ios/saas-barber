using api_barber.Models;
using api_barber.src.Requests;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace api_barber.src.Interfaces
{
    public interface IScheduleRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<Schedule> GetByIdAsync(string id);
        Task<Schedule> CreateAsync(Schedule entity);
        Task<List<Schedule>> GetAllEntitiesAsync(string barbershopId);
        Task<Schedule> UpdateAsync(Schedule entity);
        Task<Schedule> DeleteAsync(Schedule entity);
    }
}
