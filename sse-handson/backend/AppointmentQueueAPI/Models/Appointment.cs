using System.ComponentModel.DataAnnotations;

namespace AppointmentQueueAPI.Models;

/// <summary>
/// Represents an appointment in the healthcare queue system.
/// </summary>
public class Appointment
{
    /// <summary>
    /// Unique identifier for the appointment.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Name of the patient requesting the appointment.
    /// </summary>
    [Required]
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the appointment.
    /// Values: Pending → MappedToDoctor → PickedByDoctor
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Name of the doctor assigned to this appointment (if any).
    /// Set when staff maps the patient to a doctor.
    /// </summary>
    public string? AssignedDoctorName { get; set; }

    /// <summary>
    /// Name of the doctor currently examining this patient (if any).
    /// Set when doctor picks the patient from their queue.
    /// </summary>
    public string? PickedByDoctorName { get; set; }

    /// <summary>
    /// Timestamp when the appointment was booked.
    /// </summary>
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optimistic concurrency token for handling concurrent doctor picks.
    /// Maps to PostgreSQL's xmin column for row-level versioning.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// Request to book a new appointment.
/// </summary>
public class BookAppointmentRequest
{
    /// <summary>
    /// The name of the patient booking the appointment.
    /// </summary>
    [Required]
    public string PatientName { get; set; } = string.Empty;
}

/// <summary>
/// Request to assign a patient appointment to a doctor.
/// Staff role only.
/// </summary>
public class MapToDoctorRequest
{
    /// <summary>
    /// The ID of the appointment to assign.
    /// </summary>
    public int AppointmentId { get; set; }

    /// <summary>
    /// The name of the doctor to assign the patient to.
    /// </summary>
    [Required]
    public string DoctorName { get; set; } = string.Empty;
}

/// <summary>
/// Request for a doctor to pick a patient from their assigned queue.
/// Enforces concurrency: only one doctor can pick a specific patient.
/// </summary>
public class PickPatientRequest
{
    /// <summary>
    /// The ID of the appointment to pick.
    /// </summary>
    public int AppointmentId { get; set; }

    /// <summary>
    /// The name of the doctor picking the patient.
    /// Must match the AssignedDoctorName for the appointment.
    /// </summary>
    [Required]
    public string DoctorName { get; set; } = string.Empty;
}
