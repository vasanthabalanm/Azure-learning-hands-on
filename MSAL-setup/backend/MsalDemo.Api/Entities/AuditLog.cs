namespace MsalDemo.Api.Entities;

/// <summary>
/// Simple audit log for tracking actions in the system.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    public required string Action { get; set; }

    public required string PerformedBy { get; set; }

    public string? Details { get; set; }

    public DateTime Timestamp { get; set; }
}
