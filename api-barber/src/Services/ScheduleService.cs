using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Requests.Schedule;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
using MongoDB.Bson;

namespace api_barber.Services
{
    public class ScheduleService(IScheduleRepository repository) : IScheduleService
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
                    new("$addFields", new BsonDocument {
                        {"userObjectId", new BsonDocument("$toObjectId", "$barber_id")}
                    }),
                    new("$lookup", new BsonDocument {
                        {"from", "users"},
                        {"localField", "userObjectId"},
                        {"foreignField", "_id"},
                        {"as", "barberData"}
                    }),
                    new("$addFields", new BsonDocument {
                        {"barberName", new BsonDocument("$ifNull", new BsonArray {
                            new BsonDocument("$arrayElemAt", new BsonArray { "$barberData.name", 0 }),
                            "Removido"
                        })}
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"barberId", "$barber_id"},
                        {"barberName", 1},
                        {"day", 1},
                        {"startHour", "$start_hour"},
                        {"endHour", "$end_hour"},
                        {"intervalMinutes", "$interval_minutes"},
                        {"notes", 1},
                        {"active", 1},
                        {"createdAt", 1},
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
        public async Task<ResponseApi<Schedule>> GetByIdAsync(string id)
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
        public async Task<ResponseApi<Schedule>> CreateEntityAsync(Schedule entity) { await repository.CreateAsync(entity); return new(entity, 201, "Criado"); }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Schedule>> CreateAsync(CreateScheduleRequest request)
        {
            try
            {
                Schedule entity = ObjectMapper.Map<CreateScheduleRequest, Schedule>(request);
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
        public async Task<ResponseApi<Schedule>> UpdateAsync(UpdateScheduleRequest request)
        {
            try
            {
                var existed = await repository.GetByIdAsync(request.Id);
                if (existed is null) return new(null, 404, "Não encontrado");
                Schedule entity = ObjectMapper.Map<UpdateScheduleRequest, Schedule>(request);
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
        public async Task<ResponseApi<Schedule>> DeleteAsync(DeleteRequest request)
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

