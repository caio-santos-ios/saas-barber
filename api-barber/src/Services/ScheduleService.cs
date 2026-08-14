using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
namespace api_barber.Services
{
    public class ScheduleService(IScheduleRepository repository) : IScheduleService
    {
        public async Task<ResponseApi<Schedule>> CreateAsync(object request)
        {
            try
            {
                Schedule entity = ObjectMapper.Map<object, Schedule>(request);
                return await repository.CreateAsync(entity);
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<IEnumerable<Schedule>>> GetAllAsync(string barbershopId)
        {
            return await repository.GetAllAsync(barbershopId);
        }
        public async Task<ResponseApi<Schedule>> GetByIdAsync(string id, string barbershopId)
        {
            return await repository.GetByIdAsync(id, barbershopId);
        }
        public async Task<ResponseApi<Schedule>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            return await repository.SoftDeleteAsync(id, barbershopId, deletedBy);
        }
        public async Task<ResponseApi<Schedule>> UpdateAsync(string id, object request, string barbershopId)
        {
            try
            {
                var existingResponse = await repository.GetByIdAsync(id, barbershopId);
                if (existingResponse.Data == null) return new(null, 404, "Registro nÃ£o encontrado");
                Schedule entity = ObjectMapper.Map<object, Schedule>(request);
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

