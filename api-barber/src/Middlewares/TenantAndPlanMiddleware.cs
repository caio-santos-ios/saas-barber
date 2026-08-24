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
            await _next(context);
        }
    }
}

