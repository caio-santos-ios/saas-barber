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
    public class BarbershopRepository(AppDbContext appDbContext) : IBarbershopRepository
    {
        public async Task<ResponseApi<Barbershop>> CreateAsync(Barbershop entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await appDbContext.Barbershops.InsertOneAsync(entity);
                return new(entity, 201, "Registro criado com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<IEnumerable<Barbershop>>> GetAllAsync(string barbershopId)
        {
            try
            {
                var filter = Builders<Barbershop>.Filter.Eq(e => e.Deleted, false);
                if (!string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Barbershop>.Filter.Eq(e => e.Id, barbershopId);
                }
                var result = await appDbContext.Barbershops.Find(filter).ToListAsync();
                return new(result, 200, "Listagem obtida com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Barbershop>> GetByIdAsync(string id, string barbershopId)
        {
            try
            {
                var filter = Builders<Barbershop>.Filter.Eq(e => e.Id, id) & Builders<Barbershop>.Filter.Eq(e => e.Deleted, false);
                if (!string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Barbershop>.Filter.Eq(e => e.Id, barbershopId);
                }
                var entity = await appDbContext.Barbershops.Find(filter).FirstOrDefaultAsync();
                if (entity == null) return new (null, 404, "Registro nÃ£o encontrado");
                return new(entity, 200, "Registro obtido com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Barbershop>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            try
            {
                var filter = Builders<Barbershop>.Filter.Eq(e => e.Id, id);
                if (!string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Barbershop>.Filter.Eq(e => e.Id, barbershopId);
                }
                var update = Builders<Barbershop>.Update
                    .Set(e => e.Deleted, true)
                    .Set(e => e.UpdatedAt, DateTime.UtcNow)
                    .Set(e => e.UpdatedBy, deletedBy);
                var result = await appDbContext.Barbershops.UpdateOneAsync(filter, update);
                if (result.ModifiedCount == 0) return new(null, 404, "Registro nÃ£o encontrado");
                return new(null, 200, "Registro excluÃ­do com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Barbershop>> UpdateAsync(Barbershop entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                var result = await appDbContext.Barbershops.ReplaceOneAsync(e => e.Id == entity.Id, entity);
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

