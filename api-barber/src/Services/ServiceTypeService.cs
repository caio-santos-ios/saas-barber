using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Requests.ServiceType;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
using MongoDB.Bson;

namespace api_barber.Services
{
    public class ServiceTypeService(IServiceTypeRepository repository) : IServiceTypeService
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
                        {"name", 1},
                        {"description", 1},
                        {"duration", 1},
                        {"value", 1},
                        {"category", 1},
                        {"active", 1},
                        {"durationMinutes", "$duration_minutes"},
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
        public async Task<ResponseApi<ServiceType>> GetByIdAsync(string id)
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
        public async Task<ResponseApi<ServiceType>> CreateEntityAsync(ServiceType entity) { await repository.CreateAsync(entity); return new(entity, 201, "Criado"); }
        #endregion

        #region CREATE
        public async Task<ResponseApi<ServiceType>> CreateAsync(CreateServiceTypeRequest request)
        {
            try
            {
                ServiceType entity = ObjectMapper.Map<CreateServiceTypeRequest, ServiceType>(request);
                var created = await repository.CreateAsync(entity);
                return new(created, 201, "Criado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<ServiceType>> UpdateAsync(UpdateServiceTypeRequest request)
        {
            try
            {
                var existed = await repository.GetByIdAsync(request.Id);
                if (existed is null) return new(null, 404, "Não encontrado");
                ServiceType entity = ObjectMapper.Map<UpdateServiceTypeRequest, ServiceType>(request);
                entity.CreatedAt = existed.CreatedAt;
                entity.CreatedBy = existed.CreatedBy;
                var updated = await repository.UpdateAsync(entity);
                return new(updated, 200, "Atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<ServiceType>> DeleteAsync(DeleteRequest request)
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

