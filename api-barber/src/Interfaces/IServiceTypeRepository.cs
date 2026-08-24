using api_barber.Models;
using api_barber.src.Requests;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace api_barber.src.Interfaces
{
    public interface IServiceTypeRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<ServiceType> GetByIdAsync(string id);
        Task<ServiceType> CreateAsync(ServiceType entity);
        Task<List<ServiceType>> GetAllEntitiesAsync(string barbershopId);
        Task<ServiceType> UpdateAsync(ServiceType entity);
        Task<ServiceType> DeleteAsync(ServiceType entity);
    }
}
