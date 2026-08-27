using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.User;
using api_barber.src.Requests;

namespace api_barber.Interfaces
{
    public interface IUserService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<User>> GetByIdAsync(string id);
        Task<ResponseApi<User>> GetByEmailAsync(string email, string barbershopId, RoleUserEnum? role);
        Task<ResponseApi<List<dynamic>>> GetBarbersAsync(string barbershopId);
        Task<ResponseApi<List<dynamic>>> GetCustomersAsync(string barbershopId);
        Task<ResponseApi<User>> CreateAsync(CreateUserRequest request);
        Task<ResponseApi<User>> UpdateAsync(UpdateUserRequest request);
        Task<ResponseApi<User>> UpdatePasswordAsync(string userId, string newPassword);
        Task<ResponseApi<User>> DeleteAsync(DeleteRequest request);
    }
}
