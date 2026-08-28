using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;
using api_barber.src.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace api_barber.src.Repositories
{
    public class AppointmentRepository(AppDbContext appDbContext) : IAppointmentRepository
    {
        #region READ
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await appDbContext.Appointments.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> list = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return list;
        }

        public async Task<Appointment> GetByIdAsync(string id)
        {
            return await appDbContext.Appointments.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }

        public async Task<List<Appointment>> GetAllEntitiesAsync(string barbershopId) { return await appDbContext.Appointments.Find(x => !x.Deleted && x.BarbershopId == barbershopId).ToListAsync(); }

        public async Task<List<Appointment>> GetByBarberAndDateAsync(string barberId, DateTime date, string barbershopId)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var filter = Builders<Appointment>.Filter.Eq(e => e.BarberId, barberId) &
                         Builders<Appointment>.Filter.Eq(e => e.BarbershopId, barbershopId) &
                         Builders<Appointment>.Filter.Eq(e => e.Deleted, false) &
                         Builders<Appointment>.Filter.Gte(e => e.Date, startOfDay) &
                         Builders<Appointment>.Filter.Lte(e => e.Date, endOfDay);

            return await appDbContext.Appointments.Find(filter).ToListAsync();
        }

        public async Task<List<Appointment>> GetByCustomerAndDateAsync(string customerId, DateTime date, string barbershopId)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var filter = Builders<Appointment>.Filter.Eq(e => e.CustomerId, customerId) &
                 Builders<Appointment>.Filter.Eq(e => e.BarbershopId, barbershopId) &
                 Builders<Appointment>.Filter.Eq(e => e.Deleted, false) &
                 Builders<Appointment>.Filter.Gte(e => e.Date, startOfDay) &
                 Builders<Appointment>.Filter.Lte(e => e.Date, endOfDay);

            return await appDbContext.Appointments.Find(filter).ToListAsync();
        }
        #endregion

        #region CREATE
        public async Task<Appointment> CreateAsync(Appointment entity)
        {
            await appDbContext.Appointments.InsertOneAsync(entity);
            return entity;
        }
        #endregion

        #region UPDATE
        public async Task<Appointment> UpdateAsync(Appointment entity)
        {
            await appDbContext.Appointments.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        #endregion

        #region DELETE
        public async Task<Appointment> DeleteAsync(Appointment entity)
        {
            await appDbContext.Appointments.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
        #endregion
    }
}
