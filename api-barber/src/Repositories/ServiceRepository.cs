using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
namespace api_barber.src.Repositories
{
    public class ServiceRepository(AppDbContext appDbContext) : IServiceRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Services.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<Service> GetByIdAsync(string id)
        {
            return await appDbContext.Services.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<List<Service>> GetAllEntitiesAsync(string barbershopId) { return await appDbContext.Services.Find(x => !x.Deleted && x.BarbershopId == barbershopId).ToListAsync(); }
        public async Task<Service> CreateAsync(Service entity)
        {
            await appDbContext.Services.InsertOneAsync(entity);
            return entity;
        }
        public async Task<Service> UpdateAsync(Service entity)
        {
            await appDbContext.Services.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        public async Task<Service> DeleteAsync(Service entity)
        {
            await appDbContext.Services.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}
