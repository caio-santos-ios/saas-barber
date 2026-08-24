using api_barber.Models;
using api_barber.src.Requests;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
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
