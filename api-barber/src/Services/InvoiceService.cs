using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
namespace api_barber.Services
{
    public class InvoiceService(IInvoiceRepository repository) : IInvoiceService
    {
        public async Task<ResponseApi<Invoice>> CreateAsync(object request)
        {
            try
            {
                Invoice entity = ObjectMapper.Map<object, Invoice>(request);
                return await repository.CreateAsync(entity);
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<IEnumerable<Invoice>>> GetAllAsync(string barbershopId)
        {
            return await repository.GetAllAsync(barbershopId);
        }
        public async Task<ResponseApi<Invoice>> GetByIdAsync(string id, string barbershopId)
        {
            return await repository.GetByIdAsync(id, barbershopId);
        }
        public async Task<ResponseApi<Invoice>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            return await repository.SoftDeleteAsync(id, barbershopId, deletedBy);
        }
        public async Task<ResponseApi<Invoice>> UpdateAsync(string id, object request, string barbershopId)
        {
            try
            {
                var existingResponse = await repository.GetByIdAsync(id, barbershopId);
                if (existingResponse.Data == null) return new(null, 404, "Registro nÃ£o encontrado");
                Invoice entity = ObjectMapper.Map<object, Invoice>(request);
                entity.Id = id;
                if (string.IsNullOrEmpty(entity.BarbershopId)) entity.BarbershopId = barbershopId;
                entity.CreatedAt = existingResponse.Data.CreatedAt;
                entity.CreatedBy = existingResponse.Data.CreatedBy;
                return await repository.UpdateAsync(entity);
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
    }
}

