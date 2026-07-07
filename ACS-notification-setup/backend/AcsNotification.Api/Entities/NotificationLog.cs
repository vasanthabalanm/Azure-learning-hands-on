namespace AcsNotification.Api.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }

    // "FollowUp" or "Appointment"
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }

    // "Email", "SMS", "WhatsApp", "Push"
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
