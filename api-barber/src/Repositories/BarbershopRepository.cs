using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
namespace api_barber.src.Repositories
{
    public class BarbershopRepository(AppDbContext appDbContext) : IBarbershopRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Barbershops.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<Barbershop> GetByIdAsync(string id)
        {
            return await appDbContext.Barbershops.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<Barbershop?> GetByCodeAsync(string code)
        {
            return await appDbContext.Barbershops.Find(x => !x.Deleted && x.Code.Equals(code)).FirstOrDefaultAsync();
        }
        public async Task<Barbershop?> GetByAsaasCustomerIdAsync(string asaasCustomerId)
        {
            return await appDbContext.Barbershops.Find(x => !x.Deleted && x.AsaasCustomerId.Equals(asaasCustomerId)).FirstOrDefaultAsync();
        }
        public async Task<List<Barbershop>> GetAllEntitiesAsync(string barbershopId) 
        { 
            return await appDbContext.Barbershops.Find(x => !x.Deleted).ToListAsync(); 
        }
        public async Task<Barbershop> CreateAsync(Barbershop entity)
        {
            await appDbContext.Barbershops.InsertOneAsync(entity);
            return entity;
        }
        public async Task<Barbershop> UpdateAsync(Barbershop entity)
        {
            await appDbContext.Barbershops.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        public async Task<Barbershop> DeleteAsync(Barbershop entity)
        {
            await appDbContext.Barbershops.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}
