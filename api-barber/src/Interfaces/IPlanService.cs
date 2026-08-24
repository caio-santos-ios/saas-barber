using api_barber.Models;
using api_barber.Requests.Plan;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IPlanService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Plan>> GetByIdAsync(string id);
        Task<ResponseApi<Plan>> CreateEntityAsync(Plan entity);
        Task<ResponseApi<Plan>> CreateAsync(CreatePlanRequest request);
        Task<ResponseApi<Plan>> UpdateAsync(UpdatePlanRequest request);
        Task<ResponseApi<Plan>> DeleteAsync(DeleteRequest request);
    }
}
