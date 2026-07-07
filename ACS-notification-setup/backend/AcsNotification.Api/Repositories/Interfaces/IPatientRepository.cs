using AcsNotification.Api.Entities;

namespace AcsNotification.Api.Repositories.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByEmailAsync(string email);
    Task<Patient?> GetWithFollowUpsAsync(Guid patientId);
    Task<Patient?> GetWithAppointmentsAsync(Guid patientId);
}
