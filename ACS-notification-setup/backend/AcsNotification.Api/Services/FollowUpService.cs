using AcsNotification.Api.Entities;
using AcsNotification.Api.Repositories.Interfaces;
using AcsNotification.Api.Services.Interfaces;
using System.Text.Json;

namespace AcsNotification.Api.Services;

public class FollowUpService : IFollowUpService
{
    private readonly IFollowUpRepository _followUpRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<FollowUpService> _logger;

    public FollowUpService(
        IFollowUpRepository followUpRepository,
        INotificationService notificationService,
        ILogger<FollowUpService> logger)
    {
        _followUpRepository = followUpRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<FollowUp?> GetFollowUpByIdAsync(Guid id)
    {
        return await _followUpRepository.GetWithPatientAsync(id);
    }

    public async Task<IEnumerable<FollowUp>> GetPendingFollowUpsAsync()
    {
        return await _followUpRepository.GetPendingFollowUpsAsync();
    }

    public async Task<bool> TriggerNotificationAsync(Guid followUpId, CancellationToken cancellationToken = default)
    {
        var followUp = await _followUpRepository.GetWithPatientAsync(followUpId);
        if (followUp == null)
        {
            _logger.LogWarning("FollowUp {FollowUpId} not found", followUpId);
            return false;
        }

        // Parse enabled channels from JSON
        var enabledChannels = JsonSerializer.Deserialize<string[]>(followUp.EnabledChannels);
        if (enabledChannels == null || enabledChannels.Length == 0)
        {
            _logger.LogWarning("No enabled channels for FollowUp {FollowUpId}", followUpId);
            return false;
        }

        // Create message template
        var message = $"Follow-Up Reminder: {followUp.Reason} scheduled for {followUp.FollowUpDate:yyyy-MM-dd}. " +
                     $"Patient: {followUp.Patient.FirstName} {followUp.Patient.LastName}";

        // Send notifications
        var sentCount = await _notificationService.SendNotificationsAsync(
            followUp.Patient,
            enabledChannels,
            message,
            "FollowUp",
            followUp.Id,
            cancellationToken);

        // Update status if notifications were sent
        if (sentCount > 0)
        {
            followUp.Status = "Notified";
            await _followUpRepository.UpdateAsync(followUp);
        }

        return sentCount > 0;
    }
}
