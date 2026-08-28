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
        public async Task<User> GetByEmailAsync(string email, string barbershopId, RoleUserEnum? role)
        {
            if (string.IsNullOrWhiteSpace(email)) return null!;
            var cleanEmail = email.Trim();

            if (!string.IsNullOrWhiteSpace(barbershopId))
            {
                if (role != null)
                {
                    var user = await appDbContext.Users.Find(x => !x.Deleted && x.Email.Equals(cleanEmail) && x.Role == role.Value && x.BarbershopId.Equals(barbershopId.Trim())).FirstOrDefaultAsync();
                    if (user != null) return user;
                }
                var userByShop = await appDbContext.Users.Find(x => !x.Deleted && x.Email.Equals(cleanEmail) && x.BarbershopId.Equals(barbershopId.Trim())).FirstOrDefaultAsync();
                if (userByShop != null) return userByShop;
            }

            if (role != null)
            {
                var userByRole = await appDbContext.Users.Find(x => !x.Deleted && x.Email.Equals(cleanEmail) && x.Role == role.Value).FirstOrDefaultAsync();
                if (userByRole != null) return userByRole;
            }

            return await appDbContext.Users.Find(x => !x.Deleted && x.Email.Equals(cleanEmail)).FirstOrDefaultAsync();
        }
        public async Task<User> GetByEmailAdminAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null!;
            var cleanEmail = email.Trim();
            return await appDbContext.Users.Find(x => !x.Deleted && x.Email.Equals(cleanEmail) && x.Role == RoleUserEnum.Admin).FirstOrDefaultAsync();
        }
        public async Task<User> GetByDocumentAsync(string document, string barbershopId, RoleUserEnum? role)
        {
            if (string.IsNullOrWhiteSpace(document)) return null!;
            var cleanDoc = document.Trim();

            if (!string.IsNullOrWhiteSpace(barbershopId))
            {
                if (role != null)
                {
                    var user = await appDbContext.Users.Find(x => !x.Deleted && x.Document.Equals(cleanDoc) && x.Role == role.Value && x.BarbershopId.Equals(barbershopId.Trim())).FirstOrDefaultAsync();
                    if (user != null) return user;
                }
                var userByShop = await appDbContext.Users.Find(x => !x.Deleted && x.Document.Equals(cleanDoc) && x.BarbershopId.Equals(barbershopId.Trim())).FirstOrDefaultAsync();
                if (userByShop != null) return userByShop;
            }

            if (role != null)
            {
                var userByRole = await appDbContext.Users.Find(x => !x.Deleted && x.Document.Equals(cleanDoc) && x.Role == role.Value).FirstOrDefaultAsync();
                if (userByRole != null) return userByRole;
            }

            return await appDbContext.Users.Find(x => !x.Deleted && x.Document.Equals(cleanDoc)).FirstOrDefaultAsync();
        }
        public async Task<User> GetByDocumentAdminAsync(string document)
        {
            if (string.IsNullOrWhiteSpace(document)) return null!;
            var cleanDoc = document.Trim();
            return await appDbContext.Users.Find(x => !x.Deleted && x.Document.Equals(cleanDoc) && x.Role == RoleUserEnum.Admin).FirstOrDefaultAsync();
        }
        public async Task<User> GetByWhatsAppAsync(string whatsapp, string barbershopId, RoleUserEnum? role)
        {
            if (string.IsNullOrWhiteSpace(whatsapp)) return null!;
            var cleanPhone = whatsapp.Trim();

            if (!string.IsNullOrWhiteSpace(barbershopId))
            {
                if (role != null)
                {
                    var user = await appDbContext.Users.Find(x => !x.Deleted && x.WhatsApp.Equals(cleanPhone) && x.Role == role.Value && x.BarbershopId.Equals(barbershopId.Trim())).FirstOrDefaultAsync();
                    if (user != null) return user;
                }
                var userByShop = await appDbContext.Users.Find(x => !x.Deleted && x.WhatsApp.Equals(cleanPhone) && x.BarbershopId.Equals(barbershopId.Trim())).FirstOrDefaultAsync();
                if (userByShop != null) return userByShop;
            }

            if (role != null)
            {
                var userByRole = await appDbContext.Users.Find(x => !x.Deleted && x.WhatsApp.Equals(cleanPhone) && x.Role == role.Value).FirstOrDefaultAsync();
                if (userByRole != null) return userByRole;
            }

            return await appDbContext.Users.Find(x => !x.Deleted && x.WhatsApp.Equals(cleanPhone)).FirstOrDefaultAsync();
        }
        public async Task<User> GetByWhatsAppAdminAsync(string whatsapp)
        {
            if (string.IsNullOrWhiteSpace(whatsapp)) return null!;
            var cleanPhone = whatsapp.Trim();
            return await appDbContext.Users.Find(x => !x.Deleted && x.WhatsApp.Equals(cleanPhone) && x.Role == RoleUserEnum.Admin).FirstOrDefaultAsync();
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
        public async Task<User> GetAdminAsync(string barbershopId)
        {
            return await appDbContext.Users.Find(x => !x.Deleted && x.BarbershopId.Equals(barbershopId) && x.Role == RoleUserEnum.Admin).FirstOrDefaultAsync();
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