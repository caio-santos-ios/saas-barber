using api_barber.Models;
using MongoDB.Bson;
namespace api_barber.src.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<Invoice> GetByIdAsync(string id);
        Task<Invoice> CreateAsync(Invoice entity);
        Task<List<Invoice>> GetAllEntitiesAsync(string barbershopId);
        Task<Invoice> UpdateAsync(Invoice entity);
        Task<Invoice> DeleteAsync(Invoice entity);
    }
}
