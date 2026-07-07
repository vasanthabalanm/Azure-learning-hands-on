using AcsNotification.Api.Enums;

namespace AcsNotification.Api.Strategies;

// Mock implementation for Push notifications
// In production, integrate with services like Firebase Cloud Messaging, Apple Push Notification Service, or OneSignal
public class PushNotificationStrategy : INotificationStrategy
{
    private readonly ILogger<PushNotificationStrategy> _logger;

    public PushNotificationStrategy(ILogger<PushNotificationStrategy> logger)
    {
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Push;

    public async Task<bool> SendAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate sending push notification
            await Task.Delay(100, cancellationToken); // Simulate network call

            _logger.LogInformation(
                "🔔 PUSH notification sent to {Recipient}: {Message}",
                recipient, message);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to {Recipient}", recipient);
            return false;
        }
    }
}
