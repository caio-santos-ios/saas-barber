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
    public class ServiceTypeRepository(AppDbContext appDbContext) : IServiceTypeRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.ServiceTypes.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<ServiceType> GetByIdAsync(string id)
        {
            return await appDbContext.ServiceTypes.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<List<ServiceType>> GetAllEntitiesAsync(string barbershopId) { return await appDbContext.ServiceTypes.Find(x => !x.Deleted && x.BarbershopId == barbershopId).ToListAsync(); }
        public async Task<ServiceType> CreateAsync(ServiceType entity)
        {
            await appDbContext.ServiceTypes.InsertOneAsync(entity);
            return entity;
        }
        public async Task<ServiceType> UpdateAsync(ServiceType entity)
        {
            await appDbContext.ServiceTypes.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        public async Task<ServiceType> DeleteAsync(ServiceType entity)
        {
            await appDbContext.ServiceTypes.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}
