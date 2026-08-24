using api_barber.Models;
using api_barber.Requests.Invoice;
using api_barber.src.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace api_barber.Interfaces
{
    public interface IInvoiceService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId);
        Task<ResponseApi<Invoice>> GetByIdAsync(string id);
        Task<ResponseApi<Invoice>> CreateEntityAsync(Invoice entity);
        Task<ResponseApi<Invoice>> CreateAsync(CreateInvoiceRequest request);
        Task<ResponseApi<Invoice>> UpdateAsync(UpdateInvoiceRequest request);
        Task<ResponseApi<Invoice>> UpdateEntityAsync(Invoice entity);
        Task<ResponseApi<Invoice>> DeleteAsync(DeleteRequest request);
    }
}

