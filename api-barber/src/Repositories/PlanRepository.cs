using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
namespace api_barber.src.Repositories
{
    public class PlanRepository(AppDbContext appDbContext) : IPlanRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Plans.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<Plan> GetByIdAsync(string id)
        {
            return await appDbContext.Plans.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<List<Plan>> GetAllEntitiesAsync(string barbershopId) { return await appDbContext.Plans.Find(x => !x.Deleted).ToListAsync(); }
        public async Task<Plan> CreateAsync(Plan entity)
        {
            await appDbContext.Plans.InsertOneAsync(entity);
            return entity;
        }
        public async Task<Plan> UpdateAsync(Plan entity)
        {
            await appDbContext.Plans.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        public async Task<Plan> DeleteAsync(Plan entity)
        {
            await appDbContext.Plans.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}


