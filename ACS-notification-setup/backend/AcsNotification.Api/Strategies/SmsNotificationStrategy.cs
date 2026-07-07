using AcsNotification.Api.Enums;

namespace AcsNotification.Api.Strategies;

// Mock implementation for SMS notifications
// In production, integrate with services like Twilio, AWS SNS, or Azure Communication Services
public class SmsNotificationStrategy : INotificationStrategy
{
    private readonly ILogger<SmsNotificationStrategy> _logger;

    public SmsNotificationStrategy(ILogger<SmsNotificationStrategy> logger)
    {
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.SMS;

    public async Task<bool> SendAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate sending SMS
            await Task.Delay(150, cancellationToken); // Simulate network call

            _logger.LogInformation(
                "📱 SMS sent to {Recipient}: {Message}",
                recipient, message);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Recipient}", recipient);
            return false;
        }
    }
}
