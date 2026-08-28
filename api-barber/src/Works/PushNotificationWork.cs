using FirebaseAdmin.Messaging;
using MongoDB.Driver;
using api_barber.Infrastructures;

namespace api_barber.Works
{
    public class PushNotificationWork(IServiceProvider serviceProvider, ILogger<PushNotificationWork> _logger) : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(60);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDueTriggers(stoppingToken);
                    _logger.LogInformation("Background task running at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing background task.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ProcessDueTriggers(CancellationToken ct)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SendNotificationAsync(context);
        }

        private async Task SendNotificationAsync(AppDbContext context)
        {
            if (FirebaseAdmin.FirebaseApp.DefaultInstance == null)
            {
                _logger.LogWarning("FirebaseApp não está inicializado. Verifique as credenciais FCM.");
                return;
            }

            DateTime today = DateTime.UtcNow;
            
            List<Models.Notification> notifications = await context.Notifications
                .Find(x => !x.Deleted && !x.Send && x.SendAt <= today)
                .ToListAsync();

            foreach (Models.Notification notification in notifications)
            {
                try
                {
                    List<string?> rawTokens = await context.Users
                        .Find(x => x.Id == notification.UserId)
                        .Project(x => x.TokenFCM)
                        .ToListAsync();

                    List<string> tokens = rawTokens
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t!)
                        .ToList();

                    if (tokens.Count == 0)
                    {
                        _logger.LogWarning(
                            "Notificação {Id}: usuário {UserId} não tem tokens FCM registrados.",
                            notification.Id, notification.UserId);
                        continue;
                    }

                    var message = new MulticastMessage
                    {
                        Tokens = tokens,
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = notification.Title,
                            Body = notification.Message,
                        },
                        Android = new AndroidConfig
                        {
                            Priority = Priority.High,
                            Notification = new AndroidNotification
                            {
                                ChannelId = "high_importance_channel",
                                Sound = "default"
                            }
                        },
                        Apns = new ApnsConfig
                        {
                            Aps = new Aps
                            {
                                Sound = "default",
                                ContentAvailable = true
                            }
                        }
                    };

                    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

                    if (response.FailureCount > 0)
                    {
                        for (int i = 0; i < response.Responses.Count; i++)
                        {
                            if (!response.Responses[i].IsSuccess)
                            {
                                _logger.LogWarning(
                                    "Falha ao enviar pro token {Token}: {Error}",
                                    tokens[i], response.Responses[i].Exception?.Message);
                            }
                        }
                    }

                    await MarkAsSentAsync(context, notification.Id);

                    _logger.LogInformation(
                        "Notificação {Id} enviada: {Success} sucesso(s), {Failure} falha(s).",
                        notification.Id, response.SuccessCount, response.FailureCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar notificação {Id}.", notification.Id);
                }
            }
        }

        private async Task MarkAsSentAsync(AppDbContext context, string notificationId)
        {
            var update = Builders<Models.Notification>.Update.Set(x => x.Send, true);
            await context.Notifications.UpdateOneAsync(x => x.Id == notificationId, update);
        }
    }
}
