using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;

namespace api_barber.Interfaces
{
    public interface IUserService
    {
        Task<ResponseApi<IEnumerable<User>>> GetAllAsync(string barbershopId, string role = null);
        Task<ResponseApi<User>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<User>> GetByEmailAsync(string email);
        Task<ResponseApi<User>> CreateAsync(User entity);
        Task<ResponseApi<User>> UpdateAsync(string id, User entity, string barbershopId);
        Task<ResponseApi<User>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}
