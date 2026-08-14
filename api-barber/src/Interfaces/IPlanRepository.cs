using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface IPlanRepository
    {
        Task<ResponseApi<IEnumerable<Plan>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Plan>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Plan>> CreateAsync(Plan entity);
        Task<ResponseApi<Plan>> UpdateAsync(Plan entity);
        Task<ResponseApi<Plan>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

