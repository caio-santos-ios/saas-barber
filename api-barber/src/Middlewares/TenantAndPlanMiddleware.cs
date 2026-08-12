using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace api_barber.Middlewares
{
    public class TenantAndPlanMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantAndPlanMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ValidaÃ§Ã£o de plano serÃ¡ implementada aqui.
            await _next(context);
        }
    }
}
