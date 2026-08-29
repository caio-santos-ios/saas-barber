using api_barber.Interfaces;
using api_barber.Requests.Auth;
using api_barber.src.Requests;
using Microsoft.AspNetCore.Mvc;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            ResponseApi<AuthResponse> response = await authService.LoginAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        
        [HttpPost("customers/register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CreateCustomerRequest request)
        {
            ResponseApi<AuthResponse> response = await authService.RegisterCustomerAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
        
        [HttpPost("admins/register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] CreateAdminRequest request)
        {
            ResponseApi<AuthResponse> response = await authService.RegisterAdminAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            ResponseApi<ResetPasswordResponse> response = await authService.ResetPasswordAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPost("confirm-reset-password")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordRequest request)
        {
            ResponseApi<object> response = await authService.ConfirmResetPasswordAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            ResponseApi<object> response = await authService.ConfirmEmailAsync(request);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResetPasswordRequest request)
        {
            ResponseApi<object> response = await authService.ResendConfirmationEmailAsync(request.Email, request.OriginUrl);
            return StatusCode(response.Status, new { response.Data, response.Message });
        }
    }
}
