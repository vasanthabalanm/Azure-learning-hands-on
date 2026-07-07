using AcsNotification.Api.Data;
using AcsNotification.Api.Entities;
using AcsNotification.Api.Enums;
using AcsNotification.Api.Services.Interfaces;
using AcsNotification.Api.Strategies;
using System.Text.Json;

namespace AcsNotification.Api.Services;

// Orchestrates notification sending using Strategy Pattern
// Dynamically selects and executes appropriate strategies based on enabled channels
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationStrategy> _strategies;
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    // DI injects ALL registered strategies as IEnumerable<INotificationStrategy>
    public NotificationService(
        IEnumerable<INotificationStrategy> strategies,
        AppDbContext context,
        ILogger<NotificationService> logger)
    {
        _strategies = strategies;
        _context = context;
        _logger = logger;
    }

    public async Task<int> SendNotificationsAsync(
        Patient patient,
        string[] enabledChannels,
        string messageTemplate,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await SendNotificationsAsync(patient, enabledChannels, messageTemplate, entityType, entityId, null, cancellationToken);
    }

    public async Task<int> SendNotificationsAsync(
        Patient patient,
        string[] enabledChannels,
        string messageTemplate,
        string entityType,
        Guid entityId,
        object? context,
        CancellationToken cancellationToken = default)
    {
        var successCount = 0;

        _logger.LogInformation(
            "🚀 Sending notifications for {EntityType} {EntityId} to patient {PatientName}",
            entityType, entityId, $"{patient.FirstName} {patient.LastName}");

        foreach (var channelName in enabledChannels)
        {
            // Parse channel string to enum
            if (!Enum.TryParse<NotificationChannel>(channelName, out var channel))
            {
                _logger.LogWarning("Unknown channel: {Channel}", channelName);
                continue;
            }

            // Dynamically select the appropriate strategy
            var strategy = _strategies.FirstOrDefault(s => s.Channel == channel);
            if (strategy == null)
            {
                _logger.LogWarning("No strategy found for channel: {Channel}", channel);
                continue;
            }

            // Determine recipient based on channel
            var recipient = GetRecipient(patient, channel);
            if (string.IsNullOrEmpty(recipient))
            {
                _logger.LogWarning("No recipient found for channel: {Channel}", channel);
                continue;
            }

            // Execute the strategy with context if available
            bool success;
            if (context != null && strategy is EmailNotificationStrategy emailStrategy)
            {
                // Use the overload that accepts context for email
                success = await emailStrategy.SendAsync(recipient, messageTemplate, context, cancellationToken);
            }
            else
            {
                // Use the standard method for other channels
                success = await strategy.SendAsync(recipient, messageTemplate, cancellationToken);
            }

            // Determine error message
            string? errorMessage = null;
            if (!success)
            {
                errorMessage = channel == NotificationChannel.Email 
                    ? "Email strategy not configured or failed to send"
                    : "Failed to send notification";
            }

            // Log the notification attempt
            var log = new NotificationLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                Channel = channelName,
                Recipient = recipient,
                Message = messageTemplate,
                SentAt = DateTime.UtcNow,
                Success = success,
                ErrorMessage = errorMessage
            };

            await _context.NotificationLogs.AddAsync(log, cancellationToken);

            if (success)
            {
                successCount++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "✅ Sent {SuccessCount}/{TotalCount} notifications successfully",
            successCount, enabledChannels.Length);

        return successCount;
    }

    // Helper method to get recipient contact info based on channel
    private string GetRecipient(Patient patient, NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Email => patient.Email,
            NotificationChannel.SMS => patient.Phone,
            NotificationChannel.WhatsApp => patient.WhatsAppNumber ?? patient.Phone,
            NotificationChannel.Push => patient.Email, // Use email as device identifier
            _ => string.Empty
        };
    }
}
