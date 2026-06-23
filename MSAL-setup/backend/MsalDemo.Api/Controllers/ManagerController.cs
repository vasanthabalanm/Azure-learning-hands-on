using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsalDemo.Api.Authorization;

namespace MsalDemo.Api.Controllers;

/// <summary>
/// Manager-level endpoints.
/// Accessible by Admin and Manager roles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.ManagerOrAbove)]
public class ManagerController : ControllerBase
{
    /// <summary>
    /// Get team statistics dashboard.
    /// Available to Managers and Admins.
    /// </summary>
    [HttpGet("dashboard")]
    public IActionResult GetDashboard()
    {
        // Simulated dashboard data for demo purposes
        var dashboard = new
        {
            totalTeamMembers = 12,
            activeProjects = 3,
            pendingApprovals = 5,
            lastUpdated = DateTime.UtcNow
        };

        return Ok(dashboard);
    }

    /// <summary>
    /// Get reports accessible to managers.
    /// </summary>
    [HttpGet("reports")]
    public IActionResult GetReports()
    {
        var reports = new[]
        {
            new { id = 1, name = "Weekly Summary", generatedAt = DateTime.UtcNow.AddDays(-1) },
            new { id = 2, name = "Monthly Metrics", generatedAt = DateTime.UtcNow.AddDays(-7) },
            new { id = 3, name = "Quarterly Review", generatedAt = DateTime.UtcNow.AddDays(-30) }
        };

        return Ok(reports);
    }

    /// <summary>
    /// Approve a pending request (manager action).
    /// </summary>
    [HttpPost("approve/{requestId}")]
    public IActionResult ApproveRequest(int requestId)
    {
        // Simulated approval action
        return Ok(new
        {
            requestId,
            status = "Approved",
            approvedAt = DateTime.UtcNow,
            message = $"Request {requestId} has been approved."
        });
    }
}
