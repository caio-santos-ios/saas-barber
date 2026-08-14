using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
namespace api_barber.src.Repositories
{
    public class UserRepository(AppDbContext appDbContext) : IUserRepository
    {
        public async Task<ResponseApi<User>> CreateAsync(User entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await appDbContext.Users.InsertOneAsync(entity);
                return new(entity, 201, "Registro criado com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<IEnumerable<User>>> GetAllAsync(string barbershopId, string role = null)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(e => e.Deleted, false);
                var prop = typeof(User).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<User>.Filter.Eq("BarbershopId", barbershopId);
                }
                if (!string.IsNullOrEmpty(role) && Enum.TryParse(typeof(api_barber.Models.Enums.RoleUserEnum), role, true, out var roleEnum))
                {
                    filter &= Builders<User>.Filter.Eq(e => e.Role, (api_barber.Models.Enums.RoleUserEnum)roleEnum);
                }
                var result = await appDbContext.Users.Find(filter).ToListAsync();
                return new(result, 200, "Listagem obtida com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User>> GetByIdAsync(string id, string barbershopId)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(e => e.Id, id) & Builders<User>.Filter.Eq(e => e.Deleted, false);
                var prop = typeof(User).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<User>.Filter.Eq("BarbershopId", barbershopId);
                }
                var entity = await appDbContext.Users.Find(filter).FirstOrDefaultAsync();
                if (entity == null) return new (null, 404, "Registro nÃ£o encontrado");
                return new(entity, 200, "Registro obtido com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User>> GetByEmailAsync(string email)
        {
            try
            {
                var entity = await appDbContext.Users
                    .Find(e => e.Email == email && e.Deleted == false)
                    .FirstOrDefaultAsync();
                if (entity == null) return new (null, 404, "Registro não encontrado");
                
                return new(entity, 200, "Registro obtido com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<User>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(e => e.Id, id);
                var prop = typeof(User).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<User>.Filter.Eq("BarbershopId", barbershopId);
                }
                var update = Builders<User>.Update
                    .Set(e => e.Deleted, true)
                    .Set(e => e.UpdatedAt, DateTime.UtcNow)
                    .Set(e => e.UpdatedBy, deletedBy);
                var result = await appDbContext.Users.UpdateOneAsync(filter, update);
                if (result.ModifiedCount == 0) return new(null, 404, "Registro nÃ£o encontrado");
                return new(null, 200, "Registro excluÃ­do com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<User>> UpdateAsync(User entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                var result = await appDbContext.Users.ReplaceOneAsync(e => e.Id == entity.Id, entity);
                if (result.MatchedCount == 0) return new(null, 404, "Registro nÃ£o encontrado");
                return new(entity, 200, "Registro atualizado com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
    }
}

