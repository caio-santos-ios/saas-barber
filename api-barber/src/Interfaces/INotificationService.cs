using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface INotificationService
    {
        Task<ResponseApi<IEnumerable<Notification>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Notification>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Notification>> CreateAsync(object request);
        Task<ResponseApi<Notification>> UpdateAsync(string id, object request, string barbershopId);
        Task<ResponseApi<Notification>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

