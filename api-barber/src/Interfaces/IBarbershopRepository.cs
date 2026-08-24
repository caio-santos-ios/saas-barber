using api_barber.Models;
using api_barber.src.Requests;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace api_barber.src.Interfaces
{
    public interface IBarbershopRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<Barbershop> GetByIdAsync(string id);
        Task<Barbershop> CreateAsync(Barbershop entity);
        Task<List<Barbershop>> GetAllEntitiesAsync(string barbershopId);
        Task<Barbershop> UpdateAsync(Barbershop entity);
        Task<Barbershop> DeleteAsync(Barbershop entity);
    }
}
