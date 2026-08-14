using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IServiceTypeService
    {
        Task<ResponseApi<IEnumerable<ServiceType>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<ServiceType>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<ServiceType>> CreateAsync(object request);
        Task<ResponseApi<ServiceType>> UpdateAsync(string id, object request, string barbershopId);
        Task<ResponseApi<ServiceType>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

