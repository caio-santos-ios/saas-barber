using api_barber.Infrastructures;
using api_barber.Interfaces;
using api_barber.Repositories;
using api_barber.Services;
using api_barber.Handlers;
using api_barber.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IAsaasService, AsaasService>();
builder.Services.AddSingleton<FirebaseAuthHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TenantAndPlanMiddleware>();
app.MapControllers();

app.Run();
