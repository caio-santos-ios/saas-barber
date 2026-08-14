using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IPlanService
    {
        Task<ResponseApi<IEnumerable<Plan>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Plan>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Plan>> CreateAsync(object request);
        Task<ResponseApi<Plan>> UpdateAsync(string id, object request, string barbershopId);
        Task<ResponseApi<Plan>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

