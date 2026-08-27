using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.User;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
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
                        {"name", 1}
                    }),
                    new("$sort", new BsonDocument { { "createdAt", 1 } } )
                ];

                List<dynamic> barbers = await repository.GetBarbersAsync(pipeline);

                return new(barbers, 200, "Usuários listados com sucesso");
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

                return new(user, 200, "Usuários buscado com sucesso");
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

                return new(user, 200, "Usuários buscado com sucesso");
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

                return new(user, 200, "Usuários buscado com sucesso");
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
                User entity = ObjectMapper.Map<CreateUserRequest, User>(request);
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

                User entity = ObjectMapper.Map<UpdateUserRequest, User>(request);

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
