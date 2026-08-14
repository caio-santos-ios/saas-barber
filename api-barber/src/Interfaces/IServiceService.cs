using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IServiceService
    {
        Task<ResponseApi<IEnumerable<Service>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Service>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Service>> CreateAsync(object request);
        Task<ResponseApi<Service>> UpdateAsync(string id, object request, string barbershopId);
        Task<ResponseApi<Service>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

