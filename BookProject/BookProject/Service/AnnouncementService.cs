using BookProject.Data;
using BookProject.Hubss;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookProject.Service
{
    public class AnnouncementService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<AnnouncementService> _logger;

        public AnnouncementService(IServiceProvider serviceProvider, IHubContext<NotificationHub> hubContext, ILogger<AnnouncementService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var dueAnnouncements = await dbContext.Announces
                        .Where(x => !x.IsAnnounced && x.AnnouncemnetDateTime <= DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    if (dueAnnouncements?.Any() == true)
                    {
                        foreach (var announce in dueAnnouncements)
                        {
                            var notificationObject = new
                            {
                                type = "announcement",
                                content = $"{announce.Title}: {announce.Description}",
                                id = Guid.NewGuid().ToString(),
                                timestamp = DateTime.UtcNow,
                                title = announce.Title,
                                description = announce.Description
                            };

                            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notificationObject, stoppingToken);

                            announce.IsAnnounced = true;
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AnnouncementService");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
