using api_barber.Models;
using api_barber.src.Requests;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace api_barber.src.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<Notification> GetByIdAsync(string id);
        Task<Notification> CreateAsync(Notification entity);
        Task<List<Notification>> GetAllEntitiesAsync(string barbershopId);
        Task<Notification> UpdateAsync(Notification entity);
        Task<Notification> DeleteAsync(Notification entity);
    }
}
