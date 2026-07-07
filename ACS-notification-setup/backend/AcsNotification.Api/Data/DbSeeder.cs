using AcsNotification.Api.Entities;

namespace AcsNotification.Api.Data;

public class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Check if data already exists
        if (context.Patients.Any())
        {
            return; // Database already seeded
        }

        // Create sample patients
        var johnDoe = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "+1234567890",
            WhatsAppNumber = null,
            CreatedAt = DateTime.UtcNow
        };

        var janeSmith = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Phone = "+0987654321",
            WhatsAppNumber = "+0987654321",
            CreatedAt = DateTime.UtcNow
        };

        await context.Patients.AddRangeAsync(johnDoe, janeSmith);
        await context.SaveChangesAsync();

        // Create sample follow-up for John Doe (Email + SMS)
        var followUp = new FollowUp
        {
            Id = Guid.NewGuid(),
            PatientId = johnDoe.Id,
            FollowUpDate = DateTime.UtcNow.AddDays(7), // 7 days from now
            Reason = "Post-surgery follow-up consultation",
            EnabledChannels = "[\"Email\",\"SMS\"]", // JSON array
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await context.FollowUps.AddAsync(followUp);
        await context.SaveChangesAsync();

        // Create sample appointment for Jane Smith (Email + WhatsApp)
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = janeSmith.Id,
            AppointmentDate = DateTime.UtcNow.AddDays(12), // 12 days from now
            Doctor = "Dr. Sarah Johnson",
            Department = "Cardiology",
            EnabledChannels = "[\"Email\",\"WhatsApp\"]", // JSON array
            Status = "Scheduled",
            CreatedAt = DateTime.UtcNow
        };



        await context.Appointments.AddAsync(appointment);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Database seeded successfully!");
        Console.WriteLine($"   - Created 2 patients: {johnDoe.FirstName} {johnDoe.LastName}, {janeSmith.FirstName} {janeSmith.LastName}");
        Console.WriteLine($"   - Created 1 follow-up for {johnDoe.FirstName} (Email + SMS)");
        Console.WriteLine($"   - Created 1 appointment for {janeSmith.FirstName} (Email + WhatsApp)");
    }
}
