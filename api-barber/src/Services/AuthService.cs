using api_barber.Requests.User;
using api_barber.Requests.Notification;
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
    public class AuthService(IConfiguration _config, IBarbershopService _barbershopService, IAsaasService _asaasService, IUserService _userService, INotificationService _notificationService, MailHandler MailHandler) : IAuthService
    {
        public string GenerateJwtToken(string userId, string role, string barbershopId, string name = "")
        {
            string secret = Environment.GetEnvironmentVariable("JWT_KEY") ?? "";
            string issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "";
            string audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "";

            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(secret));
            SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            Claim[] claims = [
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim("userId", userId),
                new Claim("name", name),
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

                if (!user.EmailConfirmed)
                {
                    return new(null, 403, "Sua conta ainda não foi confirmada. Verifique o link de ativação enviado para o seu e-mail.");
                }

                if (!string.IsNullOrWhiteSpace(request.TokenFCM) && user.TokenFCM != request.TokenFCM)
                {
                    await _userService.UpdateTokenFcmAsync(user.Id, request.TokenFCM.Trim());
                }

                var barbershopResponse = await _barbershopService.GetByIdAsync(user.BarbershopId);
                var barbershop = barbershopResponse.Data;
                var subscriptionStatus = barbershop != null ? barbershop.SubscriptionStatus.ToString() : "Bloqueada";

                var jwt = GenerateJwtToken(user.Id, user.Role.ToString(), user.BarbershopId, user.Name);
                var authResponse = new AuthResponse
                {
                    Token = jwt,
                    Role = user.Role.ToString(),
                    BarbershopId = user.BarbershopId,
                    SubscriptionStatus = subscriptionStatus,
                    Name = user.Name,
                    Photo = user.Photo
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
                if (!string.IsNullOrWhiteSpace(request.WhatsApp))
                {
                    request.WhatsApp = ValidationUtils.CleanDigits(request.WhatsApp);
                }

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
                    BarbershopId = user.BarbershopId,
                    EmailConfirmed = false
                });

                if (createdUserRes.Data is null)
                {
                    return new(null, createdUserRes.Status, createdUserRes.Message);
                }

                string userId = createdUserRes.Data?.Id ?? user.Id;

                string confirmToken = GenerateEmailConfirmationToken(userId, user.Email);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendWelcomeConfirmationEmailAsync(user.Email, user.Name, confirmToken, request.OriginUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail de confirmação: {ex.Message}");
                    }
                });

                try
                {
                    ResponseApi<User> admin = await _userService.GetAdminAsync(user.BarbershopId);
                    if (admin.Data is not null && !string.IsNullOrEmpty(admin.Data.Id))
                    {
                        await _notificationService.CreateAsync(new CreateNotificationRequest
                        {
                            BarbershopId = user.BarbershopId,
                            CreatedBy = userId,
                            UserId = admin.Data.Id,
                            Title = "Novo Cliente Cadastrado",
                            Message = $"{user.Name} acabou de se cadastrar na sua barbearia.",
                            Read = false,
                            Send = false,
                            SendAt = DateTime.UtcNow
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao criar notificação para o admin: {ex.Message}");
                }

                var barbershopResponse = await _barbershopService.GetByIdAsync(user.BarbershopId);
                var barbershop = barbershopResponse.Data;
                var subscriptionStatus = barbershop != null ? barbershop.SubscriptionStatus.ToString() : "Ativa";

                var authResponse = new AuthResponse
                {
                    Token = string.Empty,
                    Role = user.Role.ToString(),
                    BarbershopId = user.BarbershopId,
                    SubscriptionStatus = subscriptionStatus,
                    Name = user.Name,
                    Photo = user.Photo
                };
                return new(authResponse, 201, "Cadastro realizado com sucesso! Enviamos um link de confirmação para o seu e-mail.");
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
                if (!string.IsNullOrWhiteSpace(request.WhatsApp))
                {
                    request.WhatsApp = ValidationUtils.CleanDigits(request.WhatsApp);
                }

                if (!string.IsNullOrWhiteSpace(request.Document))
                {
                    request.Document = ValidationUtils.CleanDigits(request.Document);
                }

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
                    BarbershopId = barbershop.Id,
                    EmailConfirmed = false
                });

                string adminUserId = createdAdminRes.Data?.Id ?? user.Id;

                string confirmToken = GenerateEmailConfirmationToken(adminUserId, user.Email);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendWelcomeConfirmationEmailAsync(user.Email, user.Name, confirmToken, request.OriginUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail de confirmação: {ex.Message}");
                    }
                });

                AuthResponse authResponse = new()
                {
                    Token = string.Empty,
                    Role = user.Role.ToString(),
                    BarbershopId = user.BarbershopId,
                    SubscriptionStatus = createdBarbershop.SubscriptionStatus.ToString(),
                    Name = user.Name,
                    Photo = user.Photo
                };

                return new(authResponse, 201, "Cadastro de barbearia realizado com sucesso! Enviamos um link de confirmação para o seu e-mail.");
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

                await MailHandler.SendAsync(user.Email, user.Name, "Redefinição de Senha - Na Régua", html);

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

        private string GenerateEmailConfirmationToken(string userId, string email)
        {
            var secret = Environment.GetEnvironmentVariable("JWT_KEY") ?? _config["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32Caracteres!";
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _config["Jwt:Issuer"] ?? "SaasBarbeariaIssuer";
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _config["Jwt:Audience"] ?? "SaasBarbeariaAudience";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("userId", userId),
                new Claim("email", email),
                new Claim("confirm_email", "true"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(3),
                SigningCredentials = credentials,
                Issuer = issuer,
                Audience = audience
            };
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        private async Task SendWelcomeConfirmationEmailAsync(string email, string name, string token, string originUrl)
        {
            string origin = string.IsNullOrWhiteSpace(originUrl) ? "https://saas-barber-k7nn.vercel.app" : originUrl.TrimEnd('/');
            string link = $"{origin}/confirm-email?code={token}";

            string html = $"""
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <title>Bem-vindo ao Na Régua</title>
            </head>
            <body style="margin:0;padding:0;background-color:#0f172a;font-family:'Plus Jakarta Sans',-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:#f8fafc;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#0f172a;padding:40px 16px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" style="max-width:540px;background-color:#1e293b;border-radius:16px;border:1px solid #334155;overflow:hidden;box-shadow:0 10px 25px -5px rgba(0,0,0,0.3);" cellspacing="0" cellpadding="0">
                      <tr>
                        <td style="padding:32px 32px 20px 32px;text-align:center;background:linear-gradient(180deg,#1e293b 0%,#0f172a 100%);border-bottom:1px solid #334155;">
                          <h1 style="margin:0;font-size:26px;font-weight:800;color:#d4af37;letter-spacing:-0.5px;">Na Régua</h1>
                          <p style="margin:6px 0 0 0;font-size:14px;color:#94a3b8;">Sistema de Gestão para Barbearias</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:32px;">
                          <h2 style="margin:0 0 16px 0;font-size:20px;font-weight:700;color:#f8fafc;">Olá, {name}! 👋</h2>
                          <p style="margin:0 0 16px 0;font-size:15px;line-height:1.6;color:#cbd5e1;">
                            Seja muito bem-vindo ao <strong>Na Régua</strong>! Estamos felizes em ter você conosco.
                          </p>
                          <p style="margin:0 0 24px 0;font-size:15px;line-height:1.6;color:#cbd5e1;">
                            Para começar a aproveitar todos os recursos e garantir a segurança da sua conta, por favor confirme seu endereço de e-mail clicando no botão abaixo:
                          </p>
                          <div style="text-align:center;margin:32px 0;">
                            <a href="{link}" style="display:inline-block;background-color:#d4af37;color:#000000;padding:14px 32px;text-decoration:none;border-radius:8px;font-weight:700;font-size:15px;box-shadow:0 4px 12px rgba(212,175,55,0.25);">
                              Confirmar Minha Conta
                            </a>
                          </div>
                          <p style="margin:24px 0 0 0;font-size:13px;line-height:1.5;color:#94a3b8;">
                            Se o botão acima não funcionar, copie e cole o link a seguir no seu navegador:
                          </p>
                          <p style="margin:6px 0 0 0;font-size:12px;line-height:1.4;word-break:break-all;color:#38bdf8;">
                            <a href="{link}" style="color:#38bdf8;text-decoration:none;">{link}</a>
                          </p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:20px 32px;background-color:#0f172a;border-top:1px solid #334155;text-align:center;">
                          <p style="margin:0;font-size:12px;color:#64748b;">
                            Se você não solicitou este cadastro, pode desconsiderar este e-mail.
                          </p>
                          <p style="margin:6px 0 0 0;font-size:12px;color:#64748b;">
                            © {DateTime.UtcNow.Year} Na Régua. Todos os direitos reservados.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

            Console.WriteLine($"[EMAIL CONFIRMATION LINK] Link para {email}: {link}");
            try
            {
                await MailHandler.SendAsync(email, name, "Bem-vindo ao Na Régua - Confirme seu E-mail", html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMTP ERROR] Falha no envio SMTP para {email}: {ex.Message}");
            }
        }

        public async Task<ResponseApi<object>> ResendConfirmationEmailAsync(string email, string originUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new(null, 400, "E-mail não informado.");

                var userRes = await _userService.GetByEmailAdminAsync(email.Trim());
                if (userRes.Data == null)
                {
                    userRes = await _userService.GetByEmailAsync(email.Trim(), "", null);
                }

                if (userRes.Data == null)
                    return new(null, 404, "Usuário não encontrado.");

                var user = userRes.Data;
                if (user.EmailConfirmed)
                    return new(null, 200, "Este e-mail já foi confirmado.");

                string confirmToken = GenerateEmailConfirmationToken(user.Id, user.Email);
                await SendWelcomeConfirmationEmailAsync(user.Email, user.Name, confirmToken, originUrl);

                return new(null, 200, "Link de confirmação reenviado para o seu e-mail.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro ao reenviar e-mail: {ex.Message}");
            }
        }

        public async Task<ResponseApi<object>> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Code))
                    return new(null, 400, "Código de confirmação não informado.");

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
                    principal = handler.ValidateToken(request.Code, validationParameters, out _);
                }
                catch
                {
                    return new(null, 400, "O link de confirmação é inválido ou expirou.");
                }

                var isConfirm = principal.FindFirst("confirm_email")?.Value;
                var userId = principal.FindFirst("userId")?.Value;
                var email = principal.FindFirst("email")?.Value;

                if (isConfirm != "true" || (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(email)))
                    return new(null, 400, "Token de confirmação inválido.");

                ResponseApi<User> userRes = null!;
                if (!string.IsNullOrEmpty(userId))
                {
                    userRes = await _userService.GetByIdAsync(userId);
                }
                if (userRes?.Data == null && !string.IsNullOrEmpty(email))
                {
                    userRes = await _userService.GetByEmailAsync(email, "", null);
                    if (userRes?.Data == null)
                    {
                        userRes = await _userService.GetByEmailAdminAsync(email);
                    }
                }

                if (userRes?.Data == null)
                    return new(null, 404, "Usuário não encontrado.");

                await _userService.ConfirmEmailAsync(userRes.Data.Id);

                return new(new { role = userRes.Data.Role.ToString() }, 200, "Conta confirmada e ativada com sucesso!");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado ao confirmar o e-mail: {ex.Message}");
            }
        }
    }
}


