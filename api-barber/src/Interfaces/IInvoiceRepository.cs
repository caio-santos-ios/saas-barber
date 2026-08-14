using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.src.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<ResponseApi<IEnumerable<Invoice>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Invoice>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Invoice>> CreateAsync(Invoice entity);
        Task<ResponseApi<Invoice>> UpdateAsync(Invoice entity);
        Task<ResponseApi<Invoice>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

