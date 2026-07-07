using AcsNotification.Api.Entities;

namespace AcsNotification.Api.Services.Interfaces;

public interface IFollowUpService
{
    Task<FollowUp?> GetFollowUpByIdAsync(Guid id);
    Task<IEnumerable<FollowUp>> GetPendingFollowUpsAsync();
    Task<bool> TriggerNotificationAsync(Guid followUpId, CancellationToken cancellationToken = default);
}
