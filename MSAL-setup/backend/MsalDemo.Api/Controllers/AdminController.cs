using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsalDemo.Api.Authorization;
using MsalDemo.Api.Data;
using MsalDemo.Api.Entities;
using System.Security.Claims;

namespace MsalDemo.Api.Controllers;

/// <summary>
/// Admin-only endpoints.
/// Requires the "Admin" role from Azure AD.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOnly)]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all user profiles in the system.
    /// Admin-only: view all users across tenants.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.UserProfiles
            .OrderByDescending(u => u.LastLoginAt)
            .Take(100)
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get all audit logs.
    /// Admin-only: full system audit trail.
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int take = 50)
    {
        var logs = await _db.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(take)
            .ToListAsync();

        return Ok(logs);
    }

    /// <summary>
    /// Create an audit log entry.
    /// Admin-only: manual audit entry creation.
    /// </summary>
    [HttpPost("audit-logs")]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
    {
        var performedBy = User.FindFirstValue("name")
                       ?? User.FindFirstValue(ClaimTypes.Name)
                       ?? "Unknown";

        var entry = new AuditLog
        {
            Action = request.Action,
            PerformedBy = performedBy,
            Details = request.Details,
            Timestamp = DateTime.UtcNow
        };

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAuditLogs), new { id = entry.Id }, entry);
    }
}

public record CreateAuditLogRequest(string Action, string? Details);
