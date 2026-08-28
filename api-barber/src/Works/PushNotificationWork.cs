using FirebaseAdmin.Messaging;
using MongoDB.Driver;
using api_barber.Infrastructures;
using api_barber.Models;

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

                        DateTime today = DateTime.UtcNow;

                        List<Models.Notification> notifications = await dbContext.Notifications.Find(x => !x.Deleted && !x.Send && x.SendAt <= today).ToListAsync();

                        foreach (Models.Notification notification in notifications)
                        {

                            User? user = await dbContext.Users.Find(u => u.Id == notification.UserId).FirstOrDefaultAsync(stoppingToken);

                            if(user is not null)
                            {
                                if(!string.IsNullOrEmpty(user.TokenFCM))
                                {
                                    var message = new Message()
                                    {
                                        Token = user.TokenFCM,
                                        Notification = new FirebaseAdmin.Messaging.Notification()
                                        {
                                            Title = notification.Title,
                                            Body = notification.Message
                                        }
                                    };

                                    try
                                    {
                                        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message, stoppingToken);
                                        _logger.LogInformation($"Successfully sent message to {user.Name}: {response}");


                                        notification.Send = true;
                                        await dbContext.Notifications.ReplaceOneAsync(a => a.Id == notification.Id, notification, cancellationToken: stoppingToken);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"Error sending FCM to {user.TokenFCM}: {ex.Message}");
                                    }
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


