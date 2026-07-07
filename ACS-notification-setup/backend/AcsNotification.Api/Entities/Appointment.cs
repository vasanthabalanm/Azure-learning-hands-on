namespace AcsNotification.Api.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Doctor { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;

    // Stores JSON array like ["Email", "WhatsApp"]
    public string EnabledChannels { get; set; } = string.Empty;

    // Status: "Scheduled", "Notified", "Completed", "Cancelled"
    public string Status { get; set; } = "Scheduled";
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Patient Patient { get; set; } = null!;
}
