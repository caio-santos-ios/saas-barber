using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using api_barber.Models.Enums;
namespace api_barber.src.Repositories
{
    public class UserRepository(AppDbContext appDbContext) : IUserRepository
    {
        #region READ
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Users.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<User> GetByIdAsync(string id)
        {
            return await appDbContext.Users.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<User> GetByEmailAsync(string email, RoleUserEnum? role)
        {
            if(role is not null) return await appDbContext.Users.Find(x => !x.Deleted && x.Email.Equals(email) && x.Role == role).FirstOrDefaultAsync();
            return await appDbContext.Users.Find(x => !x.Deleted && x.Email.Equals(email)).FirstOrDefaultAsync();
        }
        public async Task<List<dynamic>> GetBarbersAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Users.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        public async Task<List<dynamic>> GetCustomersAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Users.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }
        #endregion
        #region CREATE
        public async Task<User> CreateAsync(User entity)
        {
            await appDbContext.Users.InsertOneAsync(entity);
            return entity;
        }
        #endregion
        #region UPDATE
        public async Task<User> UpdateAsync(User entity)
        {
            await appDbContext.Users.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        #endregion
        #region DELETE
        public async Task<User> DeleteAsync(User entity)
        {
            await appDbContext.Users.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        #endregion
    }
}