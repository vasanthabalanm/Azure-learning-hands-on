using AcsNotification.Api.Entities;

namespace AcsNotification.Api.Services.Interfaces;

// Orchestrates notification sending across multiple channels
public interface INotificationService
{
    // Sends notifications through enabled channels for a given entity
    // Returns count of successfully sent notifications
    Task<int> SendNotificationsAsync(
        Patient patient,
        string[] enabledChannels,
        string messageTemplate,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
}
