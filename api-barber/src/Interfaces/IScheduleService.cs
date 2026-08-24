using api_barber.Models;
using api_barber.Requests.Schedule;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IScheduleService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Schedule>> GetByIdAsync(string id);
        Task<ResponseApi<Schedule>> CreateEntityAsync(Schedule entity);
        Task<ResponseApi<Schedule>> CreateAsync(CreateScheduleRequest request);
        Task<ResponseApi<Schedule>> UpdateAsync(UpdateScheduleRequest request);
        Task<ResponseApi<Schedule>> DeleteAsync(DeleteRequest request);
    }
}
