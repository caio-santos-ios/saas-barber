using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.User;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
using api_barber.Utils;
using MongoDB.Bson;

namespace api_barber.Services
{
    public class UserService(IUserRepository repository) : IUserService
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barbershopId))
                {
                    return new(new List<dynamic>(), 200, "Usuários listados com sucesso");
                }

                List<BsonDocument> pipeline =
                [
                    new("$match", new BsonDocument
                    {
                        {"deleted", false},
                        {"barbershop_id", barbershopId}
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"email", 1},
                        {"whatsapp", 1},
                        {"role", 1},
                        {"active", new BsonDocument("$ifNull", new BsonArray { "$active", true })},
                        {"createdAt", 1}
                    }),
                    new("$sort", new BsonDocument { { "createdAt", -1 } } )
                ];

                List<dynamic> users = await repository.GetAllAsync(pipeline);

                return new(users, 200, "Usuários listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<User>> GetByIdAsync(string id)
        {
            try
            {
                User user = await repository.GetByIdAsync(id);
                if (user is null) return new(null, 404, "Usuário não encontrado");

                return new(user, 200, "Usuário buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<User>> GetByEmailAsync(string email, string barbershopId, RoleUserEnum? role)
        {
            try
            {
                User user = await repository.GetByEmailAsync(email, barbershopId, role);
                if (user is null) return new(null, 404, "Usuário não encontrado");

                return new(user, 200, "Usuário buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<User>> GetByEmailAdminAsync(string email)
        {
            try
            {
                User user = await repository.GetByEmailAdminAsync(email);
                if (user is null) return new(null, 404, "Usuário não encontrado");

                return new(user, 200, "Usuário buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<User>> GetByDocumentAsync(string document, string barbershopId, RoleUserEnum? role)
        {
            try
            {
                User user = await repository.GetByDocumentAsync(document, barbershopId, role);
                if (user is null) return new(null, 404, "Usuário não encontrado");

                return new(user, 200, "Usuário buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<User>> GetByDocumentAdminAsync(string document)
        {
            try
            {
                User user = await repository.GetByDocumentAdminAsync(document);
                if (user is null) return new(null, 404, "Usuário não encontrado");

                return new(user, 200, "Usuário buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<User>> GetByWhatsAppAsync(string whatsapp, string barbershopId, RoleUserEnum? role)
        {
            try
            {
                User user = await repository.GetByWhatsAppAsync(whatsapp, barbershopId, role);
                if (user is null) return new(null, 404, "Usuário não encontrado");

                return new(user, 200, "Usuário buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<User>> GetByWhatsAppAdminAsync(string whatsapp)
        {
            try
            {
                User user = await repository.GetByWhatsAppAdminAsync(whatsapp);
                if (user is null) return new(null, 404, "Usuário não encontrado");

                return new(user, 200, "Usuário buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetBarbersAsync(string barbershopId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barbershopId))
                {
                    return new(new List<dynamic>(), 200, "Barbeiros listados com sucesso");
                }

                List<BsonDocument> pipeline =
                [
                    new("$match", new BsonDocument
                    {
                        {"deleted", false},
                        {"role", "Barber"},
                        {"barbershop_id", barbershopId}
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"email", 1},
                        {"whatsapp", 1},
                        {"document", 1},
                        {"photo", new BsonDocument("$ifNull", new BsonArray { "$photo", "" })},
                        {"active", new BsonDocument("$ifNull", new BsonArray { "$active", true })},
                        {"createdAt", 1}
                    }),
                    new("$sort", new BsonDocument { { "createdAt", 1 } } )
                ];

                List<dynamic> barbers = await repository.GetBarbersAsync(pipeline);

                return new(barbers, 200, "Barbeiros listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<List<dynamic>>> GetCustomersAsync(string barbershopId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barbershopId))
                {
                    return new(new List<dynamic>(), 200, "Clientes listados com sucesso");
                }

                List<BsonDocument> pipeline =
                [
                    new("$match", new BsonDocument
                    {
                        {"deleted", false},
                        {"role", "Customer"},
                        {"barbershop_id", barbershopId}
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"email", 1},
                        {"whatsapp", 1},
                        {"document", 1},
                        {"active", new BsonDocument("$ifNull", new BsonArray { "$active", true })},
                        {"createdAt", 1},
                    }),
                    new("$sort", new BsonDocument { { "createdAt", 1 } } )
                ];

                List<dynamic> customers = await repository.GetCustomersAsync(pipeline);

                return new(customers, 200, "Clientes listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<User>> CreateAsync(CreateUserRequest request)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    if (!ValidationUtils.IsValidEmail(request.Email))
                    {
                        return new(null, 400, "E-mail inválido.");
                    }

                    User existingEmail = request.Role == RoleUserEnum.Admin 
                        ? await repository.GetByEmailAdminAsync(request.Email.Trim())
                        : await repository.GetByEmailAsync(request.Email.Trim(), request.BarbershopId, request.Role);

                    if (existingEmail != null)
                    {
                        return new(null, 400, "Este e-mail já está cadastrado.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.WhatsApp))
                {
                    if (!ValidationUtils.IsValidPhone(request.WhatsApp))
                    {
                        return new(null, 400, "WhatsApp/Telefone inválido.");
                    }

                    User existingWhatsApp = request.Role == RoleUserEnum.Admin
                        ? await repository.GetByWhatsAppAdminAsync(request.WhatsApp.Trim())
                        : await repository.GetByWhatsAppAsync(request.WhatsApp.Trim(), request.BarbershopId, request.Role);

                    if (existingWhatsApp != null)
                    {
                        return new(null, 400, "Este WhatsApp já está cadastrado.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.Document))
                {
                    if (!ValidationUtils.IsValidDocument(request.Document))
                    {
                        return new(null, 400, "Documento (CPF/CNPJ) inválido.");
                    }

                    User existingDoc = request.Role == RoleUserEnum.Admin
                        ? await repository.GetByDocumentAdminAsync(request.Document.Trim())
                        : await repository.GetByDocumentAsync(request.Document.Trim(), request.BarbershopId, request.Role);

                    if (existingDoc != null)
                    {
                        return new(null, 400, "Este documento já está cadastrado.");
                    }
                }

                User entity = ObjectMapper.Map<CreateUserRequest, User>(request);
                if (!string.IsNullOrWhiteSpace(entity.Email))
                {
                    entity.Email = entity.Email.Trim();
                }
                if (!string.IsNullOrEmpty(entity.Password) && !entity.Password.StartsWith("$2a$") && !entity.Password.StartsWith("$2b$"))
                {
                    entity.Password = BCrypt.Net.BCrypt.HashPassword(entity.Password);
                }

                User user = await repository.CreateAsync(entity);
                if (user is null) return new(null, 400, "Falha ao criar usuário");

                return new(user, 201, "Usuário criado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<User>> UpdateAsync(UpdateUserRequest request)
        {
            try
            {
                User existedUser = await repository.GetByIdAsync(request.Id);
                if (existedUser is null) return new(null, 404, "Usuário não encontrado");

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    if (!ValidationUtils.IsValidEmail(request.Email))
                    {
                        return new(null, 400, "E-mail inválido.");
                    }

                    User existingEmail = existedUser.Role == RoleUserEnum.Admin
                        ? await repository.GetByEmailAdminAsync(request.Email.Trim())
                        : await repository.GetByEmailAsync(request.Email.Trim(), existedUser.BarbershopId, existedUser.Role);

                    if (existingEmail != null && existingEmail.Id != existedUser.Id)
                    {
                        return new(null, 400, "Este e-mail já está cadastrado.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.WhatsApp))
                {
                    if (!ValidationUtils.IsValidPhone(request.WhatsApp))
                    {
                        return new(null, 400, "WhatsApp/Telefone inválido.");
                    }

                    User existingWhatsApp = existedUser.Role == RoleUserEnum.Admin
                        ? await repository.GetByWhatsAppAdminAsync(request.WhatsApp.Trim())
                        : await repository.GetByWhatsAppAsync(request.WhatsApp.Trim(), existedUser.BarbershopId, existedUser.Role);

                    if (existingWhatsApp != null && existingWhatsApp.Id != existedUser.Id)
                    {
                        return new(null, 400, "Este WhatsApp já está cadastrado.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.Document))
                {
                    if (!ValidationUtils.IsValidDocument(request.Document))
                    {
                        return new(null, 400, "Documento (CPF/CNPJ) inválido.");
                    }

                    User existingDoc = existedUser.Role == RoleUserEnum.Admin
                        ? await repository.GetByDocumentAdminAsync(request.Document.Trim())
                        : await repository.GetByDocumentAsync(request.Document.Trim(), existedUser.BarbershopId, existedUser.Role);

                    if (existingDoc != null && existingDoc.Id != existedUser.Id)
                    {
                        return new(null, 400, "Este documento já está cadastrado.");
                    }
                }

                User entity = existedUser;

                entity.Name = request.Name;
                entity.Email = request.Email;
                entity.WhatsApp = request.WhatsApp;
                entity.Document = request.Document;

                User user = await repository.UpdateAsync(entity);
                if (user is null) return new(null, 400, "Falha ao criar usuário");

                return new(user, 200, "Usuário atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<User>> UpdatePasswordAsync(string userId, string newPassword)
        {
            try
            {
                User existedUser = await repository.GetByIdAsync(userId);
                if (existedUser is null) return new(null, 404, "Usuário não encontrado");

                if (!string.IsNullOrEmpty(newPassword) && !newPassword.StartsWith("$2a$") && !newPassword.StartsWith("$2b$"))
                {
                    existedUser.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                }
                else
                {
                    existedUser.Password = newPassword;
                }
                existedUser.PasswordResetRequired = false;

                User user = await repository.UpdateAsync(existedUser);
                if (user is null) return new(null, 400, "Falha ao atualizar senha");

                return new(user, 200, "Senha atualizada com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<User>> DeleteAsync(DeleteRequest request)
        {
            try
            {
                User existedUser = await repository.GetByIdAsync(request.Id);
                if (existedUser is null) return new(null, 404, "Usuário não encontrado");

                existedUser.Deleted = true;
                existedUser.DeletedAt = DateTime.Now;
                existedUser.DeletedBy = request.DeletedBy;

                User user = await repository.DeleteAsync(existedUser);
                if (user is null) return new(null, 400, "Falha ao criar usuário");

                return new(user, 200, "Usuário atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion
    }
}
