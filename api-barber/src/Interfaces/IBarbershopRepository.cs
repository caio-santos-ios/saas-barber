using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface IBarbershopRepository
    {
        Task<ResponseApi<IEnumerable<Barbershop>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Barbershop>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Barbershop>> CreateAsync(Barbershop entity);
        Task<ResponseApi<Barbershop>> UpdateAsync(Barbershop entity);
        Task<ResponseApi<Barbershop>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

