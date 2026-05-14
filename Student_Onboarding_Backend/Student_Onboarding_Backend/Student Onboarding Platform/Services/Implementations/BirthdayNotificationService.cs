using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Models.Enums;

namespace Student_Onboarding_Platform.Services.Implementations;

public class BirthdayNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BirthdayNotificationService> _logger;

    public BirthdayNotificationService(IServiceProvider serviceProvider, ILogger<BirthdayNotificationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Birthday Notification Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            // Schedule next run at 8:00 AM today (or tomorrow if already past)
            var nextRun = now.Date.AddHours(8);
            if (now >= nextRun)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("Next birthday check scheduled at {NextRun}", nextRun);

            await Task.Delay(delay, stoppingToken);

            await SendBirthdayWishesAsync(stoppingToken);
        }
    }

    private async Task SendBirthdayWishesAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var birthdayStudents = await userRepo.GetStudentsWithBirthdayTodayAsync();
            var count = 0;

            foreach (var student in birthdayStudents)
            {
                if (stoppingToken.IsCancellationRequested) break;

                var notification = new Notification
                {
                    UserId = student.Id,
                    Type = nameof(NotificationType.BirthdayWish),
                    Title = "Happy Birthday!",
                    Message = $"Wishing you a wonderful birthday, {student.FirstName}! 🎂 Have an amazing day filled with joy and happiness. — From the Admin Team",
                    IsRead = false
                };

                await notificationRepo.CreateAsync(notification);
                count++;

                _logger.LogInformation("Birthday wish sent to student {StudentId} ({Name})", student.Id, $"{student.FirstName} {student.LastName}");
            }

            _logger.LogInformation("Birthday wishes sent to {Count} student(s) today.", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending birthday notifications.");
        }
    }
}
