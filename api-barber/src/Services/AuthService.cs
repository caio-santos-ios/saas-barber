using api_barber.Requests.User;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.Auth;
using api_barber.src.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
namespace api_barber.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IBarbershopService _barbershopService;
        private readonly IAsaasService _asaasService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AuthService(
            IConfiguration config,
            IBarbershopService barbershopService,
            IAsaasService asaasService,
            IUserService userService,
            IEmailService emailService)
        {
            _config = config;
            _barbershopService = barbershopService;
            _asaasService = asaasService;
            _userService = userService;
            _emailService = emailService;
        }
        public string GenerateJwtToken(string userId, string role, string barbershopId)
        {
            var secret = _config["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32Caracteres!";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("role", role),
                new Claim("barbershopId", barbershopId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<ResponseApi<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var userResponse = await _userService.GetByEmailAsync(request.Email);
                var user = userResponse.Data;
                if (user == null || string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    return new (null, 401, "E-mail ou senha inválidos.");
                }

                var barbershopResponse = await _barbershopService.GetByIdAsync(user.BarbershopId);
                var barbershop = barbershopResponse.Data;
                var subscriptionStatus = barbershop != null ? barbershop.SubscriptionStatus.ToString() : "Ativa";

                var jwt = GenerateJwtToken(user.Id, user.Role.ToString(), user.BarbershopId);
                var authResponse = new AuthResponse
                {
                    Token = jwt,
                    Role = user.Role.ToString(),
                    BarbershopId = user.BarbershopId,
                    SubscriptionStatus = subscriptionStatus
                };
                return new (authResponse, 200, "Login realizado com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOGIN ERROR: " + ex.ToString());
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<AuthResponse>> RegisterCustomerAsync(CreateCustomerRequest request)
        {
            try
            {
                var existingUser = await _userService.GetByEmailAsync(request.Email);
                if (existingUser.Data != null)
                {
                    return new (null, 400, "Este e-mail já está cadastrado.");
                }

                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    WhatsApp = request.WhatsApp,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password), 
                    Role = RoleUserEnum.Customer,
                    BarbershopId = request.BarbershopId,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _userService.CreateAsync(new CreateUserRequest { Name = user.Name, Email = user.Email, WhatsApp = user.WhatsApp, Role = user.Role, DateOfBirth = user.DateOfBirth, Document = user.Document, Photo = user.Photo });

                var barbershopResponse = await _barbershopService.GetByIdAsync(user.BarbershopId);
                var barbershop = barbershopResponse.Data;
                var subscriptionStatus = barbershop != null ? barbershop.SubscriptionStatus.ToString() : "Ativa";

                var jwt = GenerateJwtToken(user.Id, user.Role.ToString(), user.BarbershopId);
                var authResponse = new AuthResponse
                {
                    Token = jwt,
                    Role = user.Role.ToString(),
                    BarbershopId = user.BarbershopId,
                    SubscriptionStatus = subscriptionStatus
                };
                return new (authResponse, 201, "Cadastro realizado com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<AuthResponse>> RegisterAdminAsync(CreateAdminRequest request)
        {
            try
            {
                var existingUser = await _userService.GetByEmailAsync(request.Email);
                if (existingUser.Data != null)
                {
                    return new (null, 400, "Este e-mail já está cadastrado.");
                }

                Enum.TryParse<TypePersonEnum>(request.TypePerson, out var typePerson);
                var asaasCustomerId = await _asaasService.CreateCustomerAsync(request.BarbershopName, request.Document, request.Email);
                var barbershop = new Barbershop
                {
                    Name = request.BarbershopName,
                    Email = request.Email,
                    Document = request.Document,
                    TypePerson = typePerson,
                    SubscriptionStatus = SubscriptionStatusEnum.Bloqueada,
                    Active = true,
                    AsaasCustomerId = asaasCustomerId,
                    WhatsApp = request.WhatsApp,
                    CreatedAt = DateTime.UtcNow
                };
                var barbershopResponse = await _barbershopService.CreateEntityAsync(barbershop);
                var createdBarbershop = (Barbershop)barbershopResponse.Data;
                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    WhatsApp = request.WhatsApp,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = RoleUserEnum.Admin,
                    BarbershopId = createdBarbershop.Id,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _userService.CreateAsync(new CreateUserRequest { Name = user.Name, Email = user.Email, WhatsApp = user.WhatsApp, Role = user.Role, DateOfBirth = user.DateOfBirth, Document = user.Document, Photo = user.Photo });
                var jwt = GenerateJwtToken(user.Id, user.Role.ToString(), user.BarbershopId);
                var authResponse = new AuthResponse
                {
                    Token = jwt,
                    Role = user.Role.ToString(),
                    BarbershopId = user.BarbershopId,
                    SubscriptionStatus = createdBarbershop.SubscriptionStatus.ToString()
                };
                return new (authResponse, 201, "Cadastro de barbearia realizado com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
        public async Task<ResponseApi<ResetPasswordResponse>> ResetPasswordAsync(string email)
        {
            try
            {
                var userResponse = await _userService.GetByEmailAsync(email);
                if (userResponse.Data == null)
                    return new(null, 404, "E-mail não encontrado.");

                var chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789!@#";
                var rng = new Random();
                var tempPassword = new string(Enumerable.Range(0, 10).Select(_ => chars[rng.Next(chars.Length)]).ToArray());

                var user = userResponse.Data;
                user.Password = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                user.PasswordResetRequired = true;
                await _userService.UpdateAsync(new UpdateUserRequest { Id = user.Id, Name = user.Name, WhatsApp = user.WhatsApp, DateOfBirth = user.DateOfBirth, Photo = user.Photo, Active = user.Active });

                var html = $"""
                    <div style="font-family:sans-serif;max-width:480px;margin:0 auto;">
                      <h2 style="color:#1e293b;">Redefinição de Senha</h2>
                      <p>Sua senha temporária foi gerada. Use-a para acessar o sistema e altere-a imediatamente após o login.</p>
                      <div style="background:#f1f5f9;border-radius:8px;padding:16px 24px;font-size:24px;font-weight:bold;letter-spacing:2px;text-align:center;color:#1e293b;">
                        {tempPassword}
                      </div>
                      <p style="color:#64748b;font-size:13px;margin-top:16px;">Se você não solicitou isso, ignore este e-mail.</p>
                    </div>
                """;

                await _emailService.SendAsync(user.Email, user.Name, "Sua nova senha - SaaS Barbearia", html);

                return new(null, 200, "Senha temporária enviada para o seu e-mail.");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
    }
}









