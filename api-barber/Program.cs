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
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "1");
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();

var firebaseKeyFile = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH") ?? "firebase-service-account.json";
var firebaseKeyPath = Path.IsPathRooted(firebaseKeyFile) ? firebaseKeyFile : Path.Combine(builder.Environment.ContentRootPath, firebaseKeyFile);
if (File.Exists(firebaseKeyPath) && FirebaseAdmin.FirebaseApp.DefaultInstance == null)
{
    FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions
    {
        Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(firebaseKeyPath)
    });
}
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddHostedService<api_barber.Works.PushNotificationWork>();

var allowedOriginsEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") ?? "";
var defaultOrigins = new List<string> { "http://localhost:4200", "http://localhost:50364", "https://saas-barber-k7nn.vercel.app" };
if (!string.IsNullOrWhiteSpace(allowedOriginsEnv))
{
    var extraOrigins = allowedOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    defaultOrigins.AddRange(extraOrigins);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(defaultOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddOpenApi();

var secret = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32Caracteres!";
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
var mongoConnection = Environment.GetEnvironmentVariable("MONGODB_CONNECTION") ?? builder.Configuration.GetConnectionString("MongoDbConnection") ?? "mongodb://localhost:27017";
var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? builder.Configuration["DatabaseSettings:DatabaseName"] ?? "BarberDb";
builder.Services.AddSingleton<MongoDB.Driver.IMongoClient>(s =>
    new MongoDB.Driver.MongoClient(mongoConnection));
builder.Services.AddScoped<MongoDB.Driver.IMongoDatabase>(s =>
    s.GetRequiredService<MongoDB.Driver.IMongoClient>().GetDatabase(databaseName));
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
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TenantAndPlanMiddleware>();
app.MapControllers();
app.Run();
