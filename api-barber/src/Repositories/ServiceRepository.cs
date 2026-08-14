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
    public class ServiceRepository(AppDbContext appDbContext) : IServiceRepository
    {
        public async Task<ResponseApi<Service>> CreateAsync(Service entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await appDbContext.Services.InsertOneAsync(entity);
                return new(entity, 201, "Registro criado com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<IEnumerable<Service>>> GetAllAsync(string barbershopId)
        {
            try
            {
                var filter = Builders<Service>.Filter.Eq(e => e.Deleted, false);
                var prop = typeof(Service).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Service>.Filter.Eq("BarbershopId", barbershopId);
                }
                var result = await appDbContext.Services.Find(filter).ToListAsync();
                return new(result, 200, "Listagem obtida com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Service>> GetByIdAsync(string id, string barbershopId)
        {
            try
            {
                var filter = Builders<Service>.Filter.Eq(e => e.Id, id) & Builders<Service>.Filter.Eq(e => e.Deleted, false);
                var prop = typeof(Service).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Service>.Filter.Eq("BarbershopId", barbershopId);
                }
                var entity = await appDbContext.Services.Find(filter).FirstOrDefaultAsync();
                if (entity == null) return new (null, 404, "Registro nÃ£o encontrado");
                return new(entity, 200, "Registro obtido com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Service>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            try
            {
                var filter = Builders<Service>.Filter.Eq(e => e.Id, id);
                var prop = typeof(Service).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Service>.Filter.Eq("BarbershopId", barbershopId);
                }
                var update = Builders<Service>.Update
                    .Set(e => e.Deleted, true)
                    .Set(e => e.UpdatedAt, DateTime.UtcNow)
                    .Set(e => e.UpdatedBy, deletedBy);
                var result = await appDbContext.Services.UpdateOneAsync(filter, update);
                if (result.ModifiedCount == 0) return new(null, 404, "Registro nÃ£o encontrado");
                return new(null, 200, "Registro excluÃ­do com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Service>> UpdateAsync(Service entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                var result = await appDbContext.Services.ReplaceOneAsync(e => e.Id == entity.Id, entity);
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

