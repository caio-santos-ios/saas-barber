using api_barber.Models;
using api_barber.Models.Enums;
using MongoDB.Bson;
namespace api_barber.src.Interfaces
{
    public interface IUserRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<List<dynamic>> GetBarbersAsync(List<BsonDocument> pipeline);
        Task<List<dynamic>> GetCustomersAsync(List<BsonDocument> pipeline);
        Task<User> GetByIdAsync(string id);
        Task<User> GetByEmailAsync(string email, string barbershopId, RoleUserEnum? role);
        Task<User> GetByEmailAdminAsync(string email);
        Task<User> GetByDocumentAsync(string document, string barbershopId, RoleUserEnum? role);
        Task<User> GetByDocumentAdminAsync(string document);
        Task<User> GetByWhatsAppAsync(string whatsapp, string barbershopId, RoleUserEnum? role);
        Task<User> GetByWhatsAppAdminAsync(string whatsapp);
        Task<User> CreateAsync(User entity);
        Task<User> UpdateAsync(User entity);
        Task<User> DeleteAsync(User entity);
    }
}