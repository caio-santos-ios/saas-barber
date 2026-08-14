using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IBarbershopService
    {
        Task<ResponseApi<IEnumerable<Barbershop>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Barbershop>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Barbershop>> CreateAsync(object request);
        Task<ResponseApi<Barbershop>> UpdateAsync(string id, object request, string barbershopId);
        Task<ResponseApi<Barbershop>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

