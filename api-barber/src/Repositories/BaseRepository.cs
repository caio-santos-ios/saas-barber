using api_barber.Interfaces;
using api_barber.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api_barber.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : ModelBase
    {
        protected readonly IMongoCollection<T> _collection;

        public BaseRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<IEnumerable<T>> GetAllAsync(string barbershopId)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Deleted, false);
            
            var type = typeof(T);
            var prop = type.GetProperty("BarbershopId");
            if (prop != null)
            {
                var barbershopFilter = Builders<T>.Filter.Eq("BarbershopId", barbershopId);
                filter = Builders<T>.Filter.And(filter, barbershopFilter);
            }

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<T> GetByIdAsync(string id, string barbershopId)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id) & Builders<T>.Filter.Eq(e => e.Deleted, false);
            var prop = typeof(T).GetProperty("BarbershopId");
            if (prop != null)
            {
                filter &= Builders<T>.Filter.Eq("BarbershopId", barbershopId);
            }

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(e => e.Id == id, entity);
        }

        public async Task SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            var prop = typeof(T).GetProperty("BarbershopId");
            if (prop != null)
            {
                filter &= Builders<T>.Filter.Eq("BarbershopId", barbershopId);
            }

            var update = Builders<T>.Update
                .Set(e => e.Deleted, true)
                .Set(e => e.UpdatedAt, DateTime.UtcNow)
                .Set(e => e.UpdatedBy, deletedBy);

            await _collection.UpdateOneAsync(filter, update);
        }
    }
}
