using AcsNotification.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcsNotification.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SeedController> _logger;

    public SeedController(AppDbContext context, ILogger<SeedController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Manual endpoint to seed database with sample data
    /// Only use this if auto-seeding didn't work
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> RunSeeding()
    {
        try
        {
            _logger.LogInformation("Manual seeding triggered via API");
            await ManualSeeder.SeedDataAsync(_context);

            return Ok(new
            {
                Message = "Database seeded successfully!",
                Instructions = "Check /api/followup/pending and /api/appointment/upcoming to verify data",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual seeding");
            return StatusCode(500, new
            {
                Message = "Seeding failed",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Check database status
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var patientCount = await _context.Patients.CountAsync();
        var followUpCount = await _context.FollowUps.CountAsync();
        var appointmentCount = await _context.Appointments.CountAsync();
        var notificationLogCount = await _context.NotificationLogs.CountAsync();

        return Ok(new
        {
            Database = _context.Database.ProviderName,
            Counts = new
            {
                Patients = patientCount,
                FollowUps = followUpCount,
                Appointments = appointmentCount,
                NotificationLogs = notificationLogCount
            },
            NeedsSeed = patientCount == 0
        });
    }
}
