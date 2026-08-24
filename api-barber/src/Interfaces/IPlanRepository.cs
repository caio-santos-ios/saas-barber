using api_barber.Models;
using api_barber.src.Requests;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace api_barber.src.Interfaces
{
    public interface IPlanRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<Plan> GetByIdAsync(string id);
        Task<Plan> CreateAsync(Plan entity);
        Task<List<Plan>> GetAllEntitiesAsync(string barbershopId);
        Task<Plan> UpdateAsync(Plan entity);
        Task<Plan> DeleteAsync(Plan entity);
    }
}
