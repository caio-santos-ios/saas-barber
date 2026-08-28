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
        Task<ResponseApi<User>> GetByEmailAdminAsync(string email);
        Task<ResponseApi<User>> GetByDocumentAsync(string document, string barbershopId, RoleUserEnum? role);
        Task<ResponseApi<User>> GetByDocumentAdminAsync(string document);
        Task<ResponseApi<User>> GetByWhatsAppAsync(string whatsapp, string barbershopId, RoleUserEnum? role);
        Task<ResponseApi<User>> GetByWhatsAppAdminAsync(string whatsapp);
        Task<ResponseApi<User>> GetAdminAsync(string barbershopId);
        Task<ResponseApi<List<dynamic>>> GetBarbersAsync(string barbershopId);
        Task<ResponseApi<List<dynamic>>> GetCustomersAsync(string barbershopId);
        Task<ResponseApi<User>> CreateAsync(CreateUserRequest request);
        Task<ResponseApi<User>> UpdateAsync(UpdateUserRequest request);
        Task<ResponseApi<User>> UpdatePasswordAsync(string userId, string newPassword);
        Task<ResponseApi<User>> UpdateTokenFcmAsync(string userId, string tokenFcm);
        Task<ResponseApi<User>> DeleteAsync(DeleteRequest request);
    }
}
