namespace NotificationFunctionApp.Models
{
    public class NotificationMessage
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? NotificationType { get; set; } // Email, SMS, Push
        public DateTime CreatedAt { get; set; }
        public string? MessageId { get; set; }
    }
}
