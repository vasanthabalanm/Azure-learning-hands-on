using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsalDemo.Api.Authorization;

namespace MsalDemo.Api.Controllers;

/// <summary>
/// User-level endpoints.
/// Accessible by any authenticated user with Admin, Manager, or User role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.UserOrAbove)]
public class UserController : ControllerBase
{
    /// <summary>
    /// Get user's personal tasks.
    /// Available to all authenticated users.
    /// </summary>
    [HttpGet("tasks")]
    public IActionResult GetMyTasks()
    {
        // Simulated task data for demo
        var tasks = new[]
        {
            new { id = 1, title = "Complete training module", status = "InProgress", dueDate = DateTime.UtcNow.AddDays(3) },
            new { id = 2, title = "Submit timesheet", status = "Pending", dueDate = DateTime.UtcNow.AddDays(1) },
            new { id = 3, title = "Review documentation", status = "Completed", dueDate = DateTime.UtcNow.AddDays(-2) }
        };

        return Ok(tasks);
    }

    /// <summary>
    /// Get user's notifications.
    /// </summary>
    [HttpGet("notifications")]
    public IActionResult GetNotifications()
    {
        var notifications = new[]
        {
            new { id = 1, message = "Your profile was updated", read = false, createdAt = DateTime.UtcNow.AddHours(-2) },
            new { id = 2, message = "New team announcement", read = true, createdAt = DateTime.UtcNow.AddDays(-1) }
        };

        return Ok(notifications);
    }

    /// <summary>
    /// Update user preferences.
    /// </summary>
    [HttpPut("preferences")]
    public IActionResult UpdatePreferences([FromBody] UserPreferencesRequest request)
    {
        // Simulated preference update
        return Ok(new
        {
            message = "Preferences updated successfully",
            preferences = request
        });
    }
}

public record UserPreferencesRequest(string? Theme, string? Language, bool EmailNotifications);
