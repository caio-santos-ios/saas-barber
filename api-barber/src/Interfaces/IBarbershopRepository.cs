using api_barber.Models;
using MongoDB.Bson;
namespace api_barber.src.Interfaces
{
    public interface IBarbershopRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<Barbershop> GetByIdAsync(string id);
        Task<Barbershop?> GetByCodeAsync(string code);
        Task<Barbershop?> GetByAsaasCustomerIdAsync(string asaasCustomerId);
        Task<Barbershop> CreateAsync(Barbershop entity);
        Task<List<Barbershop>> GetAllEntitiesAsync(string barbershopId);
        Task<Barbershop> UpdateAsync(Barbershop entity);
        Task<Barbershop> DeleteAsync(Barbershop entity);
    }
}
