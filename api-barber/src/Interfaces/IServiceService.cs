using api_barber.Models;
using api_barber.Requests.Service;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IServiceService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Service>> GetByIdAsync(string id);
        Task<ResponseApi<Service>> CreateEntityAsync(Service entity);
        Task<ResponseApi<Service>> CreateAsync(CreateServiceRequest request);
        Task<ResponseApi<Service>> UpdateAsync(UpdateServiceRequest request);
        Task<ResponseApi<Service>> DeleteAsync(DeleteRequest request);
    }
}
