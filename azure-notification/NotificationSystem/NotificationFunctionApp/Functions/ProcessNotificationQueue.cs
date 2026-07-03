using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NotificationFunctionApp.Models;
using NotificationFunctionApp.Services;

namespace NotificationFunctionApp.Functions
{
    public class ProcessNotificationQueue
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<ProcessNotificationQueue> _logger;

        public ProcessNotificationQueue(INotificationService notificationService, ILogger<ProcessNotificationQueue> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [Function("ProcessNotificationQueue")]
        public async Task Run([QueueTrigger("notification-queue", Connection = "AzureWebJobsStorage")] string queueItem)
        {
            _logger.LogInformation("=== QUEUE TRIGGER FIRED ===");

            try
            {
                _logger.LogInformation($"Queue message received. Length: {queueItem?.Length ?? 0}");
                _logger.LogInformation($"Processing queue message: {queueItem}");

                if (string.IsNullOrEmpty(queueItem))
                {
                    _logger.LogError("Queue item is null or empty");
                    return;
                }

                var notification = JsonSerializer.Deserialize<NotificationMessage>(queueItem);
                if (notification == null)
                {
                    _logger.LogError("Failed to deserialize notification message");
                    return; // Don't retry - bad message
                }

                // Validate notification type
                if (string.IsNullOrWhiteSpace(notification.NotificationType))
                {
                    _logger.LogError($"Notification type is null or empty. MessageId: {notification.MessageId}");
                    return; // Don't retry - bad message
                }

                var notificationType = notification.NotificationType.ToLower();
                _logger.LogInformation($"Processing {notificationType} notification. MessageId: {notification.MessageId}");

                bool success = notificationType switch
                {
                    "email" => await _notificationService.SendEmailAsync(notification),
                    "sms" => await _notificationService.SendSmsAsync(notification),
                    "push" => await _notificationService.SendPushNotificationAsync(notification),
                    _ => throw new InvalidOperationException($"Unknown notification type: {notification.NotificationType}")
                };

                if (success)
                {
                    _logger.LogInformation($"✓ Notification processed successfully. Type: {notificationType}, MessageId: {notification.MessageId}");
                }
                else
                {
                    _logger.LogWarning($"✗ Notification processing failed. Type: {notificationType}, MessageId: {notification.MessageId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"!!! EXCEPTION in ProcessNotificationQueue !!!");
                _logger.LogError($"Error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                _logger.LogError($"Inner exception: {ex.InnerException?.Message}");
                throw; // Re-throw to trigger Azure's retry policy
            }
        }
    }
}
