using AcsNotification.Api.Entities;

namespace AcsNotification.Api.Repositories.Interfaces;

public interface IFollowUpRepository : IRepository<FollowUp>
{
    Task<FollowUp?> GetWithPatientAsync(Guid followUpId);
    Task<IEnumerable<FollowUp>> GetPendingFollowUpsAsync();
    Task<IEnumerable<FollowUp>> GetByPatientIdAsync(Guid patientId);
}
