using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
namespace api_barber.src.Repositories
{
    public class NotificationRepository(AppDbContext appDbContext) : INotificationRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Notifications.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<Notification> GetByIdAsync(string id)
        {
            return await appDbContext.Notifications.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<List<Notification>> GetAllEntitiesAsync(string barbershopId) { return await appDbContext.Notifications.Find(x => !x.Deleted && x.BarbershopId == barbershopId).ToListAsync(); }
        public async Task<Notification> CreateAsync(Notification entity)
        {
            await appDbContext.Notifications.InsertOneAsync(entity);
            return entity;
        }
        public async Task<Notification> UpdateAsync(Notification entity)
        {
            await appDbContext.Notifications.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        public async Task<Notification> DeleteAsync(Notification entity)
        {
            await appDbContext.Notifications.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}
