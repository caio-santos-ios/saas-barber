using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface INotificationRepository
    {
        Task<ResponseApi<IEnumerable<Notification>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Notification>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Notification>> CreateAsync(Notification entity);
        Task<ResponseApi<Notification>> UpdateAsync(Notification entity);
        Task<ResponseApi<Notification>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

