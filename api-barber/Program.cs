using api_barber.Infrastructures;
using api_barber.Interfaces;
using api_barber.Services;
using api_barber.src.Interfaces;
using api_barber.src.Repositories;
using api_barber.Handlers;
using api_barber.Middlewares;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();

if (System.IO.File.Exists("firebase-service-account.json"))
{
    FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions
    {
        Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile("firebase-service-account.json")
    });
}
builder.Services.AddHostedService<api_barber.Works.PushNotificationWork>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:50364")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddOpenApi();

var secret = builder.Configuration["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32Caracteres!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<MongoDB.Driver.IMongoClient>(s =>
    new MongoDB.Driver.MongoClient(builder.Configuration.GetConnectionString("MongoDbConnection")));
builder.Services.AddScoped<MongoDB.Driver.IMongoDatabase>(s =>
    s.GetRequiredService<MongoDB.Driver.IMongoClient>().GetDatabase(builder.Configuration["DatabaseSettings:DatabaseName"] ?? "SaaSBarbearia"));
builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IBarbershopRepository, BarbershopRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IBarbershopService, BarbershopService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IServiceTypeService, ServiceTypeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddHttpClient<IAsaasService, AsaasService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddSingleton<FirebaseAuthHandler>();
builder.Services.AddControllers(options => { })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value!.Errors.Count > 0)
                .Select(e => new {
                    Field   = e.Key,
                    Message = e.Value!.Errors.First().ErrorMessage,
                    Order   = context.ActionDescriptor.Parameters
                        .SelectMany(p => p.ParameterType.GetProperties())
                        .FirstOrDefault(p => p.Name == e.Key)?
                        .GetCustomAttributes(typeof(DisplayAttribute), false)
                        .Cast<DisplayAttribute>()
                        .FirstOrDefault()?.Order ?? 999
                })
                .OrderBy(e => e.Order)
                .Select(e => new { e.Field, e.Message })
                .ToList();

            return new BadRequestObjectResult(new { errors });
        };
    });
string firebaseKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-service-account.json");
if (File.Exists(firebaseKeyPath))
{
    FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions()
    {
        Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(firebaseKeyPath)
    });
}
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TenantAndPlanMiddleware>();
app.MapControllers();
app.Run();
