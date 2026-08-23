using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FirebaseAdmin.Messaging;
using api_barber.Models;
using MongoDB.Driver;
using api_barber.Infrastructures;

namespace api_barber.Works
{
    public class PushNotificationWork : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PushNotificationWork> _logger;

        public PushNotificationWork(IServiceProvider serviceProvider, ILogger<PushNotificationWork> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PushNotificationWork started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var now = DateTime.UtcNow;
                        var soon = now.AddMinutes(15);
                        
                        var upcomingAppointments = await dbContext.Appointments.Find(a => 
                            a.Deleted == false && 
                            a.Date > now && 
                            a.Date <= soon && 
                            a.Status == api_barber.Models.Enums.AppointmentStatusEnum.Marcado
                        ).ToListAsync(stoppingToken);

                        foreach (var appt in upcomingAppointments)
                        {
                           
                            var customer = await dbContext.Users.Find(u => u.Id == appt.CustomerId).FirstOrDefaultAsync(stoppingToken);
                            if (customer != null && !string.IsNullOrEmpty(customer.TokenFCM))
                            {
                                var message = new Message()
                                {
                                    Token = customer.TokenFCM,
                                    Notification = new FirebaseAdmin.Messaging.Notification()
                                    {
                                        Title = "Lembrete de Agendamento",
                                        Body = $"Seu agendamento está próximo! Será às {appt.Date.ToLocalTime().ToString("HH:mm")}."
                                    }
                                };

                                try
                                {
                                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message, stoppingToken);
                                    _logger.LogInformation($"Successfully sent message to {customer.Name}: {response}");
                                    
                                   
                                   
                                    await dbContext.Appointments.ReplaceOneAsync(a => a.Id == appt.Id, appt, cancellationToken: stoppingToken);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error sending FCM to {customer.TokenFCM}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PushNotificationWork");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}


