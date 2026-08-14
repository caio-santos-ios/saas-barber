using api_barber.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace api_barber.Interfaces
{
    public interface IBaseRepository<T> where T : ModelBase
    {
        Task<IEnumerable<T>> GetAllAsync(string barbershopId);
        Task<T> GetByIdAsync(string id, string barbershopId);
        Task CreateAsync(T entity);
        Task UpdateAsync(string id, T entity);
        Task SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

