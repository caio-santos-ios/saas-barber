using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
namespace api_barber.src.Repositories
{
    public class InvoiceRepository(AppDbContext appDbContext) : IInvoiceRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Invoices.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<Invoice> GetByIdAsync(string id)
        {
            return await appDbContext.Invoices.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<List<Invoice>> GetAllEntitiesAsync(string barbershopId) { return await appDbContext.Invoices.Find(x => !x.Deleted && x.BarbershopId == barbershopId).ToListAsync(); }
        public async Task<Invoice> CreateAsync(Invoice entity)
        {
            await appDbContext.Invoices.InsertOneAsync(entity);
            return entity;
        }
        public async Task<Invoice> UpdateAsync(Invoice entity)
        {
            await appDbContext.Invoices.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        public async Task<Invoice> DeleteAsync(Invoice entity)
        {
            await appDbContext.Invoices.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}
