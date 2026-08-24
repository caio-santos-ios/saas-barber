using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using api_barber.src.Requests;

namespace api_barber.src.Repositories
{
    public class AppointmentRepository(AppDbContext appDbContext) : IAppointmentRepository
    {
        public async Task<ResponseApi<Appointment>> CreateAsync(Appointment entity)
        {
            try
            {
                await appDbContext.Appointments.InsertOneAsync(entity);
                return new(entity, 201, "Agendamento feito com sucesso");
            }
            catch (System.Exception ex)
            {
                return new (null, 500, ex.Message + " " + ex.StackTrace);
            }
        }

        public async Task<List<Appointment>> GetAllEntitiesAsync(string barbershopId) { return await appDbContext.Appointments.Find(x => !x.Deleted && x.BarbershopId == barbershopId).ToListAsync(); }
        public async Task<ResponseApi<List<Appointment>>> GetAllAsync(string barbershopId)
        {
            try
            {
                var result = await appDbContext.Appointments
                    .Find(e => e.BarbershopId == barbershopId && e.Deleted == false)
                    .ToListAsync();
                return new(result, 200, "Listagem obtida com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<Appointment>> GetByIdAsync(string id, string barbershopId)
        {
            try
            {
                var entity = await appDbContext.Appointments
                    .Find(e => e.Id == id && e.BarbershopId == barbershopId && e.Deleted == false)
                    .FirstOrDefaultAsync();
                
                if (entity == null) return new (null, 404, "Registro não encontrado");
                
                return new(entity, 200, "Registro obtido com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<Appointment>> UpdateAsync(Appointment entity)
        {
            try
            {
                var result = await appDbContext.Appointments.ReplaceOneAsync(e => e.Id == entity.Id, entity);
                if (result.MatchedCount == 0) return new(null, 404, "Registro não encontrado");
                
                return new(entity, 200, "Registro atualizado com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<Appointment>> SoftDeleteAsync(string id, string barbershopId, string deletedBy)
        {
            return new (null, 500, "Not implemented directly in repo, use UpdateAsync for soft delete.");
        }

        public async Task<ResponseApi<List<Appointment>>> GetByBarberAndDateAsync(string barberId, System.DateTime date, string barbershopId)
        {
            try
            {
                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                
                var filter = Builders<Appointment>.Filter.Eq(e => e.BarberId, barberId) &
                             Builders<Appointment>.Filter.Eq(e => e.BarbershopId, barbershopId) &
                             Builders<Appointment>.Filter.Eq(e => e.Deleted, false) &
                             Builders<Appointment>.Filter.Gte(e => e.Date, startOfDay) &
                             Builders<Appointment>.Filter.Lte(e => e.Date, endOfDay);

                var result = await appDbContext.Appointments.Find(filter).ToListAsync();
                return new(result, 200, "Listagem obtida com sucesso");
            }
            catch
            {
                return new (null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
    }
}

