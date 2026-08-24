using api_barber.Models;
using MongoDB.Bson;
namespace api_barber.src.Interfaces
{
    public interface IServiceRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<Service> GetByIdAsync(string id);
        Task<Service> CreateAsync(Service entity);
        Task<List<Service>> GetAllEntitiesAsync(string barbershopId);
        Task<Service> UpdateAsync(Service entity);
        Task<Service> DeleteAsync(Service entity);
    }
}
