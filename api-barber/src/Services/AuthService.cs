using api_barber.Requests.User;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.Auth;
using api_barber.src.Requests;
using api_barber.Utils;
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
            string secret = Environment.GetEnvironmentVariable("JWT_KEY") ?? "";
            string issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "";
            string audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "";

            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(secret));
            SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            Claim[] claims = [
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("role", role),
                new Claim("barbershopId", barbershopId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())                
            ];

            JwtSecurityToken token = new(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<ResponseApi<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                System.Console.WriteLine(request.Email);
                User user = new();
                if (request.Role == RoleUserEnum.Admin)
                {
                    ResponseApi<User> userResponse = await _userService.GetByEmailAdminAsync(request.Email);
                    if (userResponse.Data == null) return new(null, 400, "E-mail ou senha inválidos.");
                    user = userResponse.Data;
                }
                else
                {
                    ResponseApi<User> userResponse = await _userService.GetByEmailAsync(request.Email, request.BarbershopId, request.Role);
                    System.Console.WriteLine(userResponse.Data);
                    if (userResponse.Data == null)
                    {
                        userResponse = await _userService.GetByEmailAsync(request.Email, request.BarbershopId, null);
                    }
                    
                    if (userResponse.Data == null)
                    {
                        userResponse = await _userService.GetByEmailAdminAsync(request.Email);
                    }
                    if (userResponse.Data == null) return new(null, 400, "E-mail ou senha inválidos.");
                    user = userResponse.Data;
                }

                bool passwordValid = false;
                try
                {
                    passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                }
                catch
                {
                    passwordValid = false;
                }

                if (!passwordValid && user.Password == request.Password)
                {
                    passwordValid = true;
                    await _userService.UpdatePasswordAsync(user.Id, BCrypt.Net.BCrypt.HashPassword(request.Password));
                }

                if (string.IsNullOrEmpty(user.Password) || !passwordValid)
                {
                    return new(null, 400, "E-mail ou senha inválidos.");
                }

                if (!string.IsNullOrWhiteSpace(request.TokenFCM) && user.TokenFCM != request.TokenFCM)
                {
                    await _userService.UpdateTokenFcmAsync(user.Id, request.TokenFCM.Trim());
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
                if (!ValidationUtils.IsValidEmail(request.Email)) return new(null, 400, "E-mail inválido.");
                if (!ValidationUtils.IsValidPhone(request.WhatsApp)) return new(null, 400, "WhatsApp/Telefone inválido.");

                ResponseApi<User> existingUser = await _userService.GetByEmailAsync(request.Email, request.BarbershopId, RoleUserEnum.Customer);
                if (existingUser.Data is not null) return new(null, 400, "Este e-mail já está cadastrado.");

                ResponseApi<User> existingPhone = await _userService.GetByWhatsAppAsync(request.WhatsApp, request.BarbershopId, RoleUserEnum.Customer);
                if (existingPhone.Data is not null) return new(null, 400, "Este WhatsApp já está cadastrado.");

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

                var createdUserRes = await _userService.CreateAsync(new CreateUserRequest
                {
                    Name = user.Name,
                    Email = user.Email,
                    WhatsApp = user.WhatsApp,
                    Role = user.Role,
                    DateOfBirth = user.DateOfBirth,
                    Document = user.Document,
                    Photo = user.Photo,
                    Password = user.Password,
                    BarbershopId = user.BarbershopId
                });

                if (createdUserRes.Data is null)
                {
                    return new(null, createdUserRes.Status, createdUserRes.Message);
                }

                string userId = createdUserRes.Data?.Id ?? user.Id;

                var barbershopResponse = await _barbershopService.GetByIdAsync(user.BarbershopId);
                var barbershop = barbershopResponse.Data;
                var subscriptionStatus = barbershop != null ? barbershop.SubscriptionStatus.ToString() : "Ativa";

                var jwt = GenerateJwtToken(userId, user.Role.ToString(), user.BarbershopId);
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
                if (!ValidationUtils.IsValidEmail(request.Email)) return new(null, 400, "E-mail inválido.");
                if (!ValidationUtils.IsValidDocument(request.Document)) return new(null, 400, "Documento (CPF/CNPJ) inválido.");
                if (!ValidationUtils.IsValidPhone(request.WhatsApp)) return new(null, 400, "WhatsApp/Telefone inválido.");

                ResponseApi<User> existingUser = await _userService.GetByEmailAdminAsync(request.Email);
                if (existingUser.Data is not null) return new(null, 400, "Este e-mail já está cadastrado.");

                ResponseApi<User> existingDoc = await _userService.GetByDocumentAdminAsync(request.Document);
                if (existingDoc.Data is not null) return new(null, 400, "Este documento já está cadastrado.");

                ResponseApi<User> existingWhatsApp = await _userService.GetByWhatsAppAdminAsync(request.WhatsApp);
                if (existingWhatsApp.Data is not null) return new(null, 400, "Este WhatsApp já está cadastrado.");

                Enum.TryParse<TypePersonEnum>(request.TypePerson, out var typePerson);

                string asaasCustomerId = await _asaasService.CreateCustomerAsync(request.BarbershopName, request.Document, request.Email);
                string code = "";
                for (int i = 0; i < 10; i++)
                {
                    string candidate = Random.Shared.Next(100000, 999999).ToString();
                    var existingShop = await _barbershopService.GetByCodeAsync(candidate);
                    if (existingShop.Data == null)
                    {
                        code = candidate;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(code))
                {
                    code = Random.Shared.Next(100000, 999999).ToString();
                }

                Barbershop barbershop = new()
                {
                    Name = request.BarbershopName,
                    Email = request.Email,
                    Document = request.Document,
                    TypePerson = typePerson,
                    SubscriptionStatus = SubscriptionStatusEnum.Bloqueada,
                    Active = true,
                    Code = code,
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

                var createdAdminRes = await _userService.CreateAsync(new CreateUserRequest
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

                string adminUserId = createdAdminRes.Data?.Id ?? user.Id;
                string jwt = GenerateJwtToken(adminUserId, user.Role.ToString(), user.BarbershopId);
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
                var userResponse = await _userService.GetByEmailAsync(request.Email, "request.", null);
                if (userResponse.Data == null)
                    return new(null, 404, "E-mail não encontrado.");

                var user = userResponse.Data;
                var secret = Environment.GetEnvironmentVariable("JWT_KEY") ?? _config["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32Caracteres!";
                var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _config["Jwt:Issuer"] ?? "SaasBarbeariaIssuer";
                var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _config["Jwt:Audience"] ?? "SaasBarbeariaAudience";
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new System.Security.Claims.Claim("email", user.Email),
                    new System.Security.Claims.Claim("reset", "true"),
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = credentials,
                    Issuer = issuer,
                    Audience = audience
                };
                var handler = new JwtSecurityTokenHandler();
                var token = handler.CreateToken(tokenDescriptor);
                var jwt = handler.WriteToken(token);

                string origin = string.IsNullOrEmpty(request.OriginUrl) ? "https://app.barber.com" : request.OriginUrl;
                string link = $"{origin}/reset-password?code={jwt}";

                var html = $"""
                    <div style="font-family:sans-serif;max-width:480px;margin:0 auto;">
                      <h2 style="color:#1e293b;">Redefinição de Senha</h2>
                      <p>Você solicitou a redefinição da sua senha. Clique no botão abaixo para criar uma nova senha:</p>
                      <a href="{link}" style="display:inline-block;background:#2563eb;color:#ffffff;padding:12px 24px;text-decoration:none;border-radius:4px;font-weight:bold;margin-top:16px;">
                        Redefinir Senha
                      </a>
                      <p style="color:#64748b;font-size:13px;margin-top:16px;">Se você não solicitou isso, ignore este e-mail. Este link expira em 1 hora.</p>
                    </div>
                """;

                await _emailService.SendAsync(user.Email, user.Name, "Redefinição de Senha - SaaS Barbearia", html);

                return new(null, 200, "Link de redefinição enviado para o seu e-mail.");
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
                    return new(null, 400, "A nova senha deve ter no mínimo 6 caracteres.");

                var secret = Environment.GetEnvironmentVariable("JWT_KEY") ?? _config["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32Caracteres!";
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
                    return new(null, 400, "O link de redefinição é inválido ou expirou.");
                }

                var isReset = principal.FindFirst("reset")?.Value;
                var email = principal.FindFirst("email")?.Value;

                if (isReset != "true" || string.IsNullOrEmpty(email))
                    return new(null, 400, "Token inválido.");

                var userResponse = await _userService.GetByEmailAsync(email, "", null);
                if (userResponse.Data == null)
                    return new(null, 404, "Usuário não encontrado.");

                var user = userResponse.Data;

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


