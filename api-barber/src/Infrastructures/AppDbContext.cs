using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using api_barber.Models; // Ajuste o namespace se necessário

namespace api_barber.Infrastructures
{
    public class AppDbContext
    {
        private readonly IMongoDatabase _database;

        public AppDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase(configuration["DatabaseName"] ?? "SaaSBarbearia");
        }

        // Exemplo de como expor as collections (adicione as outras conforme a modelagem)
        // public IMongoCollection<ModelBase> AlgumaCollection => _database.GetCollection<ModelBase>("NomeDaCollection");
    }
}
