using MongoDB.Driver;
using api_barber.Models;
namespace api_barber.Infrastructures
{
    public class AppDbContext
    {
        private readonly IMongoDatabase _database;
        public AppDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDbConnection"));
            _database = client.GetDatabase(configuration["DatabaseSettings:DatabaseName"] ?? "SaaSBarbearia");
        }
        public IMongoCollection<Barbershop> Barbershops => _database.GetCollection<Barbershop>("barbershops");
        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Plan> Plans => _database.GetCollection<Plan>("plans");
        public IMongoCollection<ServiceType> ServiceTypes => _database.GetCollection<ServiceType>("services_types");
        public IMongoCollection<Service> Services => _database.GetCollection<Service>("services");
        public IMongoCollection<Schedule> Schedules => _database.GetCollection<Schedule>("schedules");
        public IMongoCollection<Appointment> Appointments => _database.GetCollection<Appointment>("appointments");
        public IMongoCollection<Invoice> Invoices => _database.GetCollection<Invoice>("invoices");
        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("notifications");
    }
}

