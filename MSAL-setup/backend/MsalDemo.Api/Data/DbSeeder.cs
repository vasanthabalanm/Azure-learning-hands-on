namespace MsalDemo.Api.Data;

/// <summary>
/// Seeds initial data for development.
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.AuditLogs.Any())
        {
            return; // Already seeded
        }

        context.AuditLogs.Add(new Entities.AuditLog
        {
            Action = "SystemStartup",
            PerformedBy = "System",
            Details = "Application initialized",
            Timestamp = DateTime.UtcNow
        });

        context.SaveChanges();
    }
}
