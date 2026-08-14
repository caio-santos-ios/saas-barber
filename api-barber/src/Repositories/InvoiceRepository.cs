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
    public class InvoiceRepository(AppDbContext appDbContext) : IInvoiceRepository
    {
        public async Task<ResponseApi<Invoice>> CreateAsync(Invoice entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await appDbContext.Invoices.InsertOneAsync(entity);
                return new(entity, 201, "Registro criado com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<IEnumerable<Invoice>>> GetAllAsync(string barbershopId)
        {
            try
            {
                var filter = Builders<Invoice>.Filter.Eq(e => e.Deleted, false);
                var prop = typeof(Invoice).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Invoice>.Filter.Eq("BarbershopId", barbershopId);
                }
                var result = await appDbContext.Invoices.Find(filter).ToListAsync();
                return new(result, 200, "Listagem obtida com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Invoice>> GetByIdAsync(string id, string barbershopId)
        {
            try
            {
                var filter = Builders<Invoice>.Filter.Eq(e => e.Id, id) & Builders<Invoice>.Filter.Eq(e => e.Deleted, false);
                var prop = typeof(Invoice).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Invoice>.Filter.Eq("BarbershopId", barbershopId);
                }
                var entity = await appDbContext.Invoices.Find(filter).FirstOrDefaultAsync();
                if (entity == null) return new (null, 404, "Registro nÃ£o encontrado");
                return new(entity, 200, "Registro obtido com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Invoice>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            try
            {
                var filter = Builders<Invoice>.Filter.Eq(e => e.Id, id);
                var prop = typeof(Invoice).GetProperty("BarbershopId");
                if (prop != null && !string.IsNullOrEmpty(barbershopId))
                {
                    filter &= Builders<Invoice>.Filter.Eq("BarbershopId", barbershopId);
                }
                var update = Builders<Invoice>.Update
                    .Set(e => e.Deleted, true)
                    .Set(e => e.UpdatedAt, DateTime.UtcNow)
                    .Set(e => e.UpdatedBy, deletedBy);
                var result = await appDbContext.Invoices.UpdateOneAsync(filter, update);
                if (result.ModifiedCount == 0) return new(null, 404, "Registro nÃ£o encontrado");
                return new(null, 200, "Registro excluÃ­do com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<Invoice>> UpdateAsync(Invoice entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                var result = await appDbContext.Invoices.ReplaceOneAsync(e => e.Id == entity.Id, entity);
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

