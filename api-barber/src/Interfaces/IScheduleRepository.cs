using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface IScheduleRepository
    {
        Task<ResponseApi<IEnumerable<Schedule>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Schedule>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Schedule>> CreateAsync(Schedule entity);
        Task<ResponseApi<Schedule>> UpdateAsync(Schedule entity);
        Task<ResponseApi<Schedule>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

