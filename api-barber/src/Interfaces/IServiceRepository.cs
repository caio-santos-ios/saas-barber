using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface IServiceRepository
    {
        Task<ResponseApi<IEnumerable<Service>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Service>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Service>> CreateAsync(Service entity);
        Task<ResponseApi<Service>> UpdateAsync(Service entity);
        Task<ResponseApi<Service>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

