using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Models;
using api_barber.src.Requests;
namespace api_barber.Interfaces
{
    public interface IInvoiceService
    {
        Task<ResponseApi<IEnumerable<Invoice>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Invoice>> GetByIdAsync(string id, string barbershopId);
        Task<ResponseApi<Invoice>> CreateAsync(object request);
        Task<ResponseApi<Invoice>> UpdateAsync(string id, object request, string barbershopId);
        Task<ResponseApi<Invoice>> SoftDeleteAsync(string id, string barbershopId, string deletedBy);
    }
}

