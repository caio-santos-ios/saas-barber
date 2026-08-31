using api_barber.Models;
using api_barber.Requests.Barbershop;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IBarbershopService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Barbershop>> GetByIdAsync(string id);
        Task<ResponseApi<Barbershop>> GetByCodeAsync(string code);
        Task<ResponseApi<Barbershop>> GetByAsaasCustomerIdAsync(string asaasCustomerId);
        Task<ResponseApi<Barbershop>> CreateEntityAsync(Barbershop entity);
        Task<ResponseApi<Barbershop>> CreateAsync(CreateBarbershopRequest request);
        Task<ResponseApi<Barbershop>> UpdateAsync(UpdateBarbershopRequest request);
        Task<ResponseApi<Barbershop>> UpdateEntityAsync(Barbershop entity);
        Task<ResponseApi<Barbershop>> DeleteAsync(DeleteRequest request);
    }
}
