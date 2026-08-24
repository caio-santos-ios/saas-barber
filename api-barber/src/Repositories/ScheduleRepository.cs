using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace api_barber.src.Repositories
{
    public class ScheduleRepository(AppDbContext appDbContext) : IScheduleRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Schedules.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<Schedule> GetByIdAsync(string id)
        {
            return await appDbContext.Schedules.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<List<Schedule>> GetAllEntitiesAsync(string barbershopId) { return await appDbContext.Schedules.Find(x => !x.Deleted && x.BarbershopId == barbershopId).ToListAsync(); }
        public async Task<Schedule> CreateAsync(Schedule entity)
        {
            await appDbContext.Schedules.InsertOneAsync(entity);
            return entity;
        }
        public async Task<Schedule> UpdateAsync(Schedule entity)
        {
            await appDbContext.Schedules.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        public async Task<Schedule> DeleteAsync(Schedule entity)
        {
            await appDbContext.Schedules.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}
