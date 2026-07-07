using AcsNotification.Api.Entities;

namespace AcsNotification.Api.Repositories.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<Appointment?> GetWithPatientAsync(Guid appointmentId);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync();
    Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId);
}
