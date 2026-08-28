using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Requests.Barbershop;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
using MongoDB.Bson;

namespace api_barber.Services
{
    public class BarbershopService(IBarbershopRepository repository) : IBarbershopService
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId)
        {
            try
            {
                var matchDoc = new BsonDocument { { "deleted", false } };
                if (!string.IsNullOrWhiteSpace(barbershopId) && ObjectId.TryParse(barbershopId, out var objId))
                {
                    matchDoc.Add("_id", objId);
                }

                List<BsonDocument> pipeline =
                [
                    new("$match", matchDoc),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"name", 1},
                        {"email", 1},
                        {"phone", 1},
                        {"document", 1},
                        {"logo", 1},
                        {"address", 1},
                        {"planId", new BsonDocument("$ifNull", new BsonArray { "$planId", "$plan_id" })},
                        {"subscriptionStatus", new BsonDocument("$ifNull", new BsonArray { "$subscriptionStatus", "$subscription_status" })},
                        {"code", 1},
                        {"createdAt", 1}
                    }),
                    new("$sort", new BsonDocument { { "createdAt", -1 } } )
                ];
                List<dynamic> list = await repository.GetAllAsync(pipeline);
                return new(list, 200, "Listado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<Barbershop>> GetByIdAsync(string id)
        {
            try
            {
                var entity = await repository.GetByIdAsync(id);
                if (entity is null) return new(null, 404, "Não encontrado");
                return new(entity, 200, "Buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<Barbershop>> GetByCodeAsync(string code)
        {
            try
            {
                var entity = await repository.GetByCodeAsync(code);
                if (entity is null) return new(null, 404, "Barbearia não encontrada para este código.");
                return new(entity, 200, "Barbearia encontrada com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<Barbershop>> CreateEntityAsync(Barbershop entity) { await repository.CreateAsync(entity); return new(entity, 201, "Criado"); }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Barbershop>> CreateAsync(CreateBarbershopRequest request)
        {
            try
            {
                Barbershop entity = ObjectMapper.Map<CreateBarbershopRequest, Barbershop>(request);
                var created = await repository.CreateAsync(entity);
                return new(created, 201, "Criado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<Barbershop>> UpdateEntityAsync(Barbershop entity)
        {
            await repository.UpdateAsync(entity);
            return new(entity, 200, "OK");
        }
        #endregion

        public async Task<ResponseApi<Barbershop>> UpdateAsync(UpdateBarbershopRequest request)
        {
            try
            {
                var id = !string.IsNullOrEmpty(request.Id) ? request.Id : request.BarbershopId;
                var existed = await repository.GetByIdAsync(id);
                if (existed is null) return new(null, 404, "Não encontrado");

                if (!string.IsNullOrEmpty(request.Name)) existed.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Document)) existed.Document = request.Document;
                if (!string.IsNullOrEmpty(request.Phone)) existed.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.WhatsApp)) existed.WhatsApp = request.WhatsApp;
                if (!string.IsNullOrEmpty(request.Email)) existed.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Logo)) existed.Logo = request.Logo;
                if (!string.IsNullOrEmpty(request.Code)) existed.Code = request.Code;
                if (request.Address != null) existed.Address = request.Address;

                existed.UpdatedAt = DateTime.Now;
                existed.UpdatedBy = request.UpdatedBy;

                var updated = await repository.UpdateAsync(existed);
                return new(updated, 200, "Atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }

        #region DELETE
        public async Task<ResponseApi<Barbershop>> DeleteAsync(DeleteRequest request)
        {
            try
            {
                var existed = await repository.GetByIdAsync(request.Id);
                if (existed is null) return new(null, 404, "Não encontrado");
                existed.Deleted = true;
                existed.DeletedAt = DateTime.Now;
                existed.DeletedBy = request.DeletedBy;
                var deleted = await repository.DeleteAsync(existed);
                return new(deleted, 200, "Excluido com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }

        #endregion
    }
}

