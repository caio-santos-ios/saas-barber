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
        Task<User> GetByEmailAsync(string email, RoleUserEnum? role);
        Task<User> CreateAsync(User entity);
        Task<User> UpdateAsync(User entity);
        Task<User> DeleteAsync(User entity);
    }
}