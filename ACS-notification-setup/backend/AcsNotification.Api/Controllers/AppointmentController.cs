using AcsNotification.Api.DTOs;
using AcsNotification.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcsNotification.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(
        IAppointmentService appointmentService,
        ILogger<AppointmentController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get appointment details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(Guid id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound(new { Message = $"Appointment with ID {id} not found" });
        }

        return Ok(new
        {
            appointment.Id,
            appointment.PatientId,
            Patient = new
            {
                appointment.Patient.Id,
                appointment.Patient.FirstName,
                appointment.Patient.LastName,
                appointment.Patient.Email,
                appointment.Patient.Phone,
                appointment.Patient.WhatsAppNumber
            },
            appointment.AppointmentDate,
            appointment.Doctor,
            appointment.Department,
            appointment.EnabledChannels,
            appointment.Status
        });
    }

    /// <summary>
    /// Get all upcoming appointments
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingAppointments()
    {
        var appointments = await _appointmentService.GetUpcomingAppointmentsAsync();
        return Ok(appointments.Select(a => new
        {
            a.Id,
            a.PatientId,
            PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
            a.AppointmentDate,
            a.Doctor,
            a.Department,
            a.EnabledChannels,
            a.Status
        }));
    }

    /// <summary>
    /// Trigger notification for a specific appointment
    /// Demo: Sends Email + WhatsApp notifications
    /// </summary>
    [HttpPost("trigger-notification")]
    public async Task<IActionResult> TriggerNotification([FromBody] TriggerAppointmentNotificationDto request)
    {
        _logger.LogInformation("Triggering notification for Appointment {AppointmentId}", request.AppointmentId);

        var success = await _appointmentService.TriggerNotificationAsync(request.AppointmentId);

        if (!success)
        {
            return BadRequest(new { 
                Message = "No notifications were sent successfully. This could be because: " +
                         "1) Appointment doesn't exist, " +
                         "2) No channels are enabled, or " +
                         "3) All notification channels failed (e.g., Email not configured). " +
                         "Check logs and NotificationLogs table for details."
            });
        }

        return Ok(new
        {
            Message = "Notification triggered successfully (at least one channel succeeded)",
            AppointmentId = request.AppointmentId,
            Timestamp = DateTime.UtcNow
        });
    }
}
