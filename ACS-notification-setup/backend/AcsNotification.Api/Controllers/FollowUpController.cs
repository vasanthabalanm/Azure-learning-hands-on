using AcsNotification.Api.DTOs;
using AcsNotification.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AcsNotification.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowUpController : ControllerBase
{
    private readonly IFollowUpService _followUpService;
    private readonly ILogger<FollowUpController> _logger;

    public FollowUpController(
        IFollowUpService followUpService,
        ILogger<FollowUpController> logger)
    {
        _followUpService = followUpService;
        _logger = logger;
    }

    /// <summary>
    /// Get follow-up details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFollowUp(Guid id)
    {
        var followUp = await _followUpService.GetFollowUpByIdAsync(id);
        if (followUp == null)
        {
            return NotFound(new { Message = $"FollowUp with ID {id} not found" });
        }

        return Ok(new
        {
            followUp.Id,
            followUp.PatientId,
            Patient = new
            {
                followUp.Patient.Id,
                followUp.Patient.FirstName,
                followUp.Patient.LastName,
                followUp.Patient.Email,
                followUp.Patient.Phone
            },
            followUp.FollowUpDate,
            followUp.Reason,
            followUp.EnabledChannels,
            followUp.Status
        });
    }

    /// <summary>
    /// Get all pending follow-ups
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingFollowUps()
    {
        var followUps = await _followUpService.GetPendingFollowUpsAsync();
        return Ok(followUps.Select(f => new
        {
            f.Id,
            f.PatientId,
            PatientName = $"{f.Patient.FirstName} {f.Patient.LastName}",
            f.FollowUpDate,
            f.Reason,
            f.EnabledChannels,
            f.Status
        }));
    }

    /// <summary>
    /// Trigger notification for a specific follow-up
    /// Demo: Sends SMS + Email notifications
    /// </summary>
    [HttpPost("trigger-notification")]
    public async Task<IActionResult> TriggerNotification([FromBody] TriggerFollowUpNotificationDto request)
    {
        _logger.LogInformation("Triggering notification for FollowUp {FollowUpId}", request.FollowUpId);

        var success = await _followUpService.TriggerNotificationAsync(request.FollowUpId);

        if (!success)
        {
            return BadRequest(new { Message = "Failed to trigger notification. Check if FollowUp exists and has enabled channels." });
        }

        return Ok(new
        {
            Message = "Notification triggered successfully",
            FollowUpId = request.FollowUpId,
            Timestamp = DateTime.UtcNow
        });
    }
}
