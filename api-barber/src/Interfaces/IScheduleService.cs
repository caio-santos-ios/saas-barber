using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IScheduleService
    {
        Task<ResponseApi<IEnumerable<Schedule>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Schedule>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Schedule>> CreateAsync(object request);
        Task<ResponseApi<Schedule>> UpdateAsync(string id, object request, string barbershopId);
        Task<ResponseApi<Schedule>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

