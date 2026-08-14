using api_barber.Infrastructures;
using api_barber.Interfaces;
using api_barber.Repositories;
using api_barber.Services;
using api_barber.src.Interfaces;
using api_barber.src.Repositories;
using api_barber.Handlers;
using api_barber.Middlewares;
var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:65303")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddOpenApi();
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
var firebaseKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-service-account.json");
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
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TenantAndPlanMiddleware>();
app.MapControllers();
app.Run();

