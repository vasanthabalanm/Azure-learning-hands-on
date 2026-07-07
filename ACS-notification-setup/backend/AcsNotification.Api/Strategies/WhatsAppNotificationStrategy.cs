using AcsNotification.Api.Enums;

namespace AcsNotification.Api.Strategies;

// Mock implementation for WhatsApp notifications
// In production, integrate with WhatsApp Business API or Azure Communication Services
public class WhatsAppNotificationStrategy : INotificationStrategy
{
    private readonly ILogger<WhatsAppNotificationStrategy> _logger;

    public WhatsAppNotificationStrategy(ILogger<WhatsAppNotificationStrategy> logger)
    {
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.WhatsApp;

    public async Task<bool> SendAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate sending WhatsApp message
            await Task.Delay(200, cancellationToken); // Simulate network call

            _logger.LogInformation(
                "💬 WHATSAPP sent to {Recipient}: {Message}",
                recipient, message);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {Recipient}", recipient);
            return false;
        }
    }
}
