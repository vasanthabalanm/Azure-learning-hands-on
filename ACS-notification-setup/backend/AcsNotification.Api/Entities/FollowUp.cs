namespace AcsNotification.Api.Entities;

public class FollowUp
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime FollowUpDate { get; set; }
    public string Reason { get; set; } = string.Empty;

    // Stores JSON array like ["Email", "SMS"]
    public string EnabledChannels { get; set; } = string.Empty;

    // Status: "Pending", "Notified", "Completed"
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Patient Patient { get; set; } = null!;
}
