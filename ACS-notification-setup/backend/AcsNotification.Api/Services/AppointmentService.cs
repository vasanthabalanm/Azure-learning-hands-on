using AcsNotification.Api.Entities;
using AcsNotification.Api.Repositories.Interfaces;
using AcsNotification.Api.Services.Interfaces;
using System.Text.Json;

namespace AcsNotification.Api.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        INotificationService notificationService,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(Guid id)
    {
        return await _appointmentRepository.GetWithPatientAsync(id);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync()
    {
        return await _appointmentRepository.GetUpcomingAppointmentsAsync();
    }

    public async Task<bool> TriggerNotificationAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetWithPatientAsync(appointmentId);
        if (appointment == null)
        {
            _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
            return false;
        }

        // Parse enabled channels from JSON
        var enabledChannels = JsonSerializer.Deserialize<string[]>(appointment.EnabledChannels);
        if (enabledChannels == null || enabledChannels.Length == 0)
        {
            _logger.LogWarning("No enabled channels for Appointment {AppointmentId}", appointmentId);
            return false;
        }

        // Create message template
        var message = $"Appointment Reminder: {appointment.Department} with {appointment.Doctor} " +
                     $"on {appointment.AppointmentDate:yyyy-MM-dd HH:mm}. " +
                     $"Patient: {appointment.Patient.FirstName} {appointment.Patient.LastName}";

        // Send notifications
        var sentCount = await _notificationService.SendNotificationsAsync(
            appointment.Patient,
            enabledChannels,
            message,
            "Appointment",
            appointment.Id,
            cancellationToken);

        // Update status if notifications were sent
        if (sentCount > 0)
        {
            appointment.Status = "Notified";
            await _appointmentRepository.UpdateAsync(appointment);
        }

        return sentCount > 0;
    }
}
