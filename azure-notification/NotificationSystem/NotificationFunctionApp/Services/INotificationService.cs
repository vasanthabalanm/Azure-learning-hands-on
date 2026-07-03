using NotificationFunctionApp.Models;

namespace NotificationFunctionApp.Services
{
    public interface INotificationService
    {
        Task<bool> SendEmailAsync(NotificationMessage message);
        Task<bool> SendSmsAsync(NotificationMessage message);
        Task<bool> SendPushNotificationAsync(NotificationMessage message);
    }
}
