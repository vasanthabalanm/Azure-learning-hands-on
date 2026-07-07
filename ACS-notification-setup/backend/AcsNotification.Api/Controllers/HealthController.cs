using Microsoft.AspNetCore.Mvc;

namespace AcsNotification.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "ACS Notification API",
            Timestamp = DateTime.UtcNow
        });
    }
}
