using api_barber.Models;
using api_barber.Requests.Notification;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface INotificationService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Notification>> GetByIdAsync(string id);
        Task<ResponseApi<Notification>> CreateAsync(CreateNotificationRequest request);
        Task<ResponseApi<List<Notification>>> CreateManyAsync(List<CreateNotificationRequest> request);
        Task<ResponseApi<Notification>> UpdateAsync(UpdateNotificationRequest request);
        Task<ResponseApi<Notification>> DeleteAsync(DeleteRequest request);
    }
}
