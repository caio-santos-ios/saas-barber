using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface IServiceTypeRepository
    {
        Task<ResponseApi<IEnumerable<ServiceType>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<ServiceType>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<ServiceType>> CreateAsync(ServiceType entity);
        Task<ResponseApi<ServiceType>> UpdateAsync(ServiceType entity);
        Task<ResponseApi<ServiceType>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

