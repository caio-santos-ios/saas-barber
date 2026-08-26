using api_barber.Requests.User;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.Auth;
using api_barber.src.Requests;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace api_barber.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IBarbershopService _barbershopService;
        private readonly IAsaasService _asaasService;
        private readonly IUserService _userService;
        private readonly api_barber.src.Interfaces.IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public AuthService(api_barber.src.Interfaces.IUserRepository userRepository,
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
            _userRepository = userRepository;
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
                    return new(null, 401, "E-mail ou senha inválidos.");
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
                return new(authResponse, 200, "Login realizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<AuthResponse>> RegisterCustomerAsync(CreateCustomerRequest request)
        {
            try
            {
                var existingUser = await _userService.GetByEmailAsync(request.Email);
                if (existingUser.Data != null)
                {
                    return new(null, 400, "Este e-mail já está cadastrado.");
                }

                User user = new()
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
                await _userService.CreateAsync(new CreateUserRequest { Name = user.Name, Email = user.Email, WhatsApp = user.WhatsApp, Role = user.Role, DateOfBirth = user.DateOfBirth, Document = user.Document, Photo = user.Photo, Password = user.Password });

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
                return new(authResponse, 201, "Cadastro realizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<AuthResponse>> RegisterAdminAsync(CreateAdminRequest request)
        {
            try
            {
                ResponseApi<User> existingUser = await _userService.GetByEmailAsync(request.Email);
                if (existingUser.Data is not null) return new(null, 400, "Este e-mail já está cadastrado.");

                Enum.TryParse<TypePersonEnum>(request.TypePerson, out var typePerson);

                string asaasCustomerId = await _asaasService.CreateCustomerAsync(request.BarbershopName, request.Document, request.Email);
                Barbershop barbershop = new()
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

                ResponseApi<Barbershop> barbershopResponse = await _barbershopService.CreateEntityAsync(barbershop);
                if (barbershopResponse.Data is null) return new(null, 400, "Não foi possivel criar a conta");

                Barbershop createdBarbershop = barbershopResponse.Data;

                User user = new()
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

                await _userService.CreateAsync(new CreateUserRequest
                {
                    Name = user.Name,
                    Email = user.Email,
                    WhatsApp = user.WhatsApp,
                    Role = user.Role,
                    DateOfBirth = user.DateOfBirth,
                    Document = user.Document,
                    Photo = user.Photo,
                    Password = user.Password,
                    BarbershopId = barbershop.Id
                });

                string jwt = GenerateJwtToken(user.Id, user.Role.ToString(), user.BarbershopId);
                AuthResponse authResponse = new()
                {
                    Token = jwt,
                    Role = user.Role.ToString(),
                    BarbershopId = user.BarbershopId,
                    SubscriptionStatus = createdBarbershop.SubscriptionStatus.ToString()
                };

                return new(authResponse, 201, "Cadastro de barbearia realizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var userResponse = await _userService.GetByEmailAsync(request.Email);
                if (userResponse.Data == null)
                    return new(null, 404, "E-mail n�o encontrado.");

                var user = userResponse.Data;

                var secret = _config["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32Caracteres!";
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim("email", user.Email),
                    new Claim("reset", "true"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = credentials,
                    Issuer = _config["Jwt:Issuer"],
                    Audience = _config["Jwt:Audience"]
                };
                var handler = new JwtSecurityTokenHandler();
                var token = handler.CreateToken(tokenDescriptor);
                var jwt = handler.WriteToken(token);

                string origin = string.IsNullOrEmpty(request.OriginUrl) ? "https://app.barber.com" : request.OriginUrl;
                string link = $"{origin}/reset-password?code={jwt}";

                var html = $"""
                    <div style="font-family:sans-serif;max-width:480px;margin:0 auto;">
                      <h2 style="color:#1e293b;">Redefini��o de Senha</h2>
                      <p>Voc� solicitou a redefini��o da sua senha. Clique no bot�o abaixo para criar uma nova senha:</p>
                      <a href="{link}" style="display:inline-block;background:#2563eb;color:#ffffff;padding:12px 24px;text-decoration:none;border-radius:4px;font-weight:bold;margin-top:16px;">
                        Redefinir Senha
                      </a>
                      <p style="color:#64748b;font-size:13px;margin-top:16px;">Se voc� n�o solicitou isso, ignore este e-mail. Este link expira em 1 hora.</p>
                    </div>
                """;

                await _emailService.SendAsync(user.Email, user.Name, "Redefini��o de Senha - SaaS Barbearia", html);

                return new(null, 200, "Link de redefini��o enviado para o seu e-mail.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<object>> ConfirmResetPasswordAsync(ConfirmResetPasswordRequest request)
        {
            try
            {
                if (request.NewPassword.Length < 6)
                    return new(null, 400, "A nova senha deve ter no m�nimo 6 caracteres.");

                var secret = _config["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32Caracteres!";
                var handler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                ClaimsPrincipal principal;
                try
                {
                    principal = handler.ValidateToken(request.Code, validationParameters, out var validatedToken);
                }
                catch
                {
                    return new(null, 400, "O link de redefini��o � inv�lido ou expirou.");
                }

                var isReset = principal.FindFirst("reset")?.Value;
                var email = principal.FindFirst("email")?.Value;

                if (isReset != "true" || string.IsNullOrEmpty(email))
                    return new(null, 400, "Token inv�lido.");

                var userResponse = await _userService.GetByEmailAsync(email);
                if (userResponse.Data == null)
                    return new(null, 404, "Usu�rio n�o encontrado.");

                var user = userResponse.Data;



                // Salva no banco. Como IUserService.UpdateAsync aceita UpdateUserRequest, precisamos apenas chamar.
                // Mas de acordo com a arquitetura, eu n�o deveria mexer no UserService.
                // Vou chamar o UserRepository diretamente ou o UpdateAsync.
                await _userService.UpdatePasswordAsync(user.Id, BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

                return new(null, 200, "Senha redefinida com sucesso.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
    }
}





