using api_barber.Requests.Auth;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(string userId, string role, string barbershopId, string name = "");
        Task<ResponseApi<AuthResponse>> LoginAsync(LoginRequest request);
        Task<ResponseApi<AuthResponse>> RegisterCustomerAsync(CreateCustomerRequest request);
        Task<ResponseApi<AuthResponse>> RegisterAdminAsync(CreateAdminRequest request);
        Task<ResponseApi<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ResponseApi<object>> ConfirmResetPasswordAsync(ConfirmResetPasswordRequest request);
        Task<ResponseApi<object>> ConfirmEmailAsync(ConfirmEmailRequest request);
        Task<ResponseApi<object>> ResendConfirmationEmailAsync(string email, string originUrl);
    }
}
