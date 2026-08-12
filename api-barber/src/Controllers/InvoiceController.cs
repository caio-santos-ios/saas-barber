using api_barber.Interfaces;
using api_barber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using api_barber.Requests.Finance;
using api_barber.Models.Enums;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("invoices")]
    public class InvoiceController : ControllerBase
    {
        private readonly IBaseRepository<Invoice> _repo;

        public InvoiceController(IBaseRepository<Invoice> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string barbershopId)
        {
            var result = await _repo.GetAllAsync(barbershopId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, [FromQuery] string barbershopId)
        {
            var entity = new Invoice
            {
                Date = System.DateTime.UtcNow,
                DueDate = request.DueDate,
                Value = request.Value,
                PaymentMethod = request.PaymentMethod,
                Description = request.Description,
                Status = InvoiceStatusEnum.EmAberto,
                BarbershopId = barbershopId
            };
            await _repo.CreateAsync(entity);
            return Ok(entity);
        }
    }
}
