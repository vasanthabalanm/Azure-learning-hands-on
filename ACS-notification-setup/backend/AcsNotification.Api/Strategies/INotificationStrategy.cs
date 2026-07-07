using AcsNotification.Api.Enums;

namespace AcsNotification.Api.Strategies;

// Strategy Pattern Interface
// Each concrete strategy implements sending notifications via different channels
// This follows the Open/Closed Principle - new channels can be added without modifying existing code
public interface INotificationStrategy
{
    // Identifies which channel this strategy handles
    NotificationChannel Channel { get; }

    // Sends notification asynchronously
    // Returns true if sent successfully, false otherwise
    Task<bool> SendAsync(string recipient, string message, CancellationToken cancellationToken = default);
}
