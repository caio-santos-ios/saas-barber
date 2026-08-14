using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
namespace api_barber.Services
{
    public class BarbershopService(IBarbershopRepository repository) : IBarbershopService
    {
        public async Task<ResponseApi<Barbershop>> CreateAsync(object request)
        {
            try
            {
                Barbershop entity = ObjectMapper.Map<object, Barbershop>(request);
                return await repository.CreateAsync(entity);
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<IEnumerable<Barbershop>>> GetAllAsync(string barbershopId)
        {
            return await repository.GetAllAsync(barbershopId);
        }
        public async Task<ResponseApi<Barbershop>> GetByIdAsync(string id, string barbershopId)
        {
            return await repository.GetByIdAsync(id, barbershopId);
        }
        public async Task<ResponseApi<Barbershop>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            return await repository.SoftDeleteAsync(id, barbershopId, deletedBy);
        }
        public async Task<ResponseApi<Barbershop>> UpdateAsync(string id, object request, string barbershopId)
        {
            try
            {
                var existingResponse = await repository.GetByIdAsync(id, barbershopId);
                if (existingResponse.Data == null) return new(null, 404, "Registro nÃ£o encontrado");
                Barbershop entity = ObjectMapper.Map<object, Barbershop>(request);
                entity.Id = id;
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

