using api_barber.Models;
using api_barber.Requests.ServiceType;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IServiceTypeService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<ServiceType>> GetByIdAsync(string id);
        Task<ResponseApi<ServiceType>> CreateEntityAsync(ServiceType entity);
        Task<ResponseApi<ServiceType>> CreateAsync(CreateServiceTypeRequest request);
        Task<ResponseApi<ServiceType>> UpdateAsync(UpdateServiceTypeRequest request);
        Task<ResponseApi<ServiceType>> DeleteAsync(DeleteRequest request);
    }
}
