using AcsNotification.Api.Entities;

namespace AcsNotification.Api.Services.Interfaces;

public interface IAppointmentService
{
    Task<Appointment?> GetAppointmentByIdAsync(Guid id);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync();
    Task<bool> TriggerNotificationAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}
