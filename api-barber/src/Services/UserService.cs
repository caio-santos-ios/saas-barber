using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
namespace api_barber.Services
{
    public class UserService(IUserRepository repository) : IUserService
    {
        public async Task<ResponseApi<User>> CreateAsync(User entity)
        {
            try
            {
                return await repository.CreateAsync(entity);
            }
            catch (System.Exception ex)
            {
                return new (null, 500, "Erro: " + ex.Message);
            }
        }
        public async Task<ResponseApi<IEnumerable<User>>> GetAllAsync(string barbershopId, string role = null)
        {
            return await repository.GetAllAsync(barbershopId, role);
        }
        public async Task<ResponseApi<User>> GetByIdAsync(string id, string barbershopId)
        {
            return await repository.GetByIdAsync(id, barbershopId);
        }

        public async Task<ResponseApi<User>> GetByEmailAsync(string email)
        {
            return await repository.GetByEmailAsync(email);
        }
        public async Task<ResponseApi<User>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            return await repository.SoftDeleteAsync(id, barbershopId, deletedBy);
        }
        public async Task<ResponseApi<User>> UpdateAsync(string id, User entity, string barbershopId)
        {
            try
            {
                var existingResponse = await repository.GetByIdAsync(id, barbershopId);
                if (existingResponse.Data == null) return new(null, 404, "Registro nÃ£o encontrado");
                
                entity.Id = id;
                if (string.IsNullOrEmpty(entity.BarbershopId)) entity.BarbershopId = barbershopId;
                entity.CreatedAt = existingResponse.Data.CreatedAt;
                entity.CreatedBy = existingResponse.Data.CreatedBy;
                return await repository.UpdateAsync(entity);
            }
            catch (System.Exception ex)
            {
                return new (null, 500, "Erro: " + ex.Message);
            }
        }
    }
}

