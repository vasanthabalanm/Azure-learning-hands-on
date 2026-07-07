namespace AcsNotification.Api.Entities;

public class Patient
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
