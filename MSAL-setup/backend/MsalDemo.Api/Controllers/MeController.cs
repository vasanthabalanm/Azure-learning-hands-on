using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MsalDemo.Api.Data;
using MsalDemo.Api.Entities;
using System.Security.Claims;

namespace MsalDemo.Api.Controllers;

/// <summary>
/// Returns information about the currently authenticated user.
/// Accessible by any authenticated user regardless of role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly AppDbContext _db;

    public MeController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get current user's claims and profile.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCurrentUser()
    {
        var objectId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                    ?? User.FindFirstValue("oid");
        var tenantId = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid")
                    ?? User.FindFirstValue("tid");
        var email = User.FindFirstValue(ClaimTypes.Email)
                 ?? User.FindFirstValue("preferred_username");
        var name = User.FindFirstValue("name")
                ?? User.FindFirstValue(ClaimTypes.Name);
        
        // Try multiple claim types for roles
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!roles.Any())
        {
            roles = User.FindAll("roles").Select(c => c.Value).ToList();
        }

        if (string.IsNullOrEmpty(objectId))
        {
            return BadRequest("Unable to identify user from token.");
        }

        // Upsert user profile in local database
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(u => u.ObjectId == objectId);
        if (profile == null)
        {
            profile = new UserProfile
            {
                ObjectId = objectId,
                TenantId = tenantId,
                Email = email,
                DisplayName = name,
                FirstLoginAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            _db.UserProfiles.Add(profile);
        }
        else
        {
            profile.LastLoginAt = DateTime.UtcNow;
            profile.Email = email;
            profile.DisplayName = name;
        }
        await _db.SaveChangesAsync();

        return Ok(new
        {
            objectId,
            tenantId,
            email,
            displayName = name,
            roles,
            localProfileId = profile.Id,
            firstLogin = profile.FirstLoginAt,
            lastLogin = profile.LastLoginAt
        });
    }
    
    /// <summary>
    /// Debug endpoint to show all claims in the token.
    /// </summary>
    [HttpGet("claims")]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var isInAdminRole = User.IsInRole("Admin");
        return Ok(new { claims, isInAdminRole });
    }
}
