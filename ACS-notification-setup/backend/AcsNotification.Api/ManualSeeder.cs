using AcsNotification.Api.Data;
using AcsNotification.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcsNotification.Api;

/// <summary>
/// Manual seeding utility - Run this if auto-seeding didn't work
/// Usage: Call this from a controller or run as a separate endpoint
/// </summary>
public static class ManualSeeder
{
    public static async Task SeedDataAsync(AppDbContext context)
    {
        Console.WriteLine("🌱 Starting manual seed...");

        // Clear existing data first (optional - only if you want to reset)
        // Uncomment these lines if you want to clear and re-seed
        // context.NotificationLogs.RemoveRange(context.NotificationLogs);
        // context.FollowUps.RemoveRange(context.FollowUps);
        // context.Appointments.RemoveRange(context.Appointments);
        // context.Patients.RemoveRange(context.Patients);
        // await context.SaveChangesAsync();

        // Check if patients already exist
        if (await context.Patients.AnyAsync())
        {
            Console.WriteLine("⚠️  Patients already exist. Skipping seed.");
            return;
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
        Console.WriteLine($"✅ Created patients: {johnDoe.FirstName} {johnDoe.LastName}, {janeSmith.FirstName} {janeSmith.LastName}");

        // Create sample follow-up for John Doe (Email + SMS)
        var followUp = new FollowUp
        {
            Id = Guid.NewGuid(),
            PatientId = johnDoe.Id,
            FollowUpDate = DateTime.UtcNow.AddDays(7),
            Reason = "Post-surgery follow-up consultation",
            EnabledChannels = "[\"Email\",\"SMS\"]",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await context.FollowUps.AddAsync(followUp);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Created follow-up for {johnDoe.FirstName} (Email + SMS)");

        // Create sample appointment for Jane Smith (Email + WhatsApp)
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = janeSmith.Id,
            AppointmentDate = DateTime.UtcNow.AddDays(12),
            Doctor = "Dr. Sarah Johnson",
            Department = "Cardiology",
            EnabledChannels = "[\"Email\",\"WhatsApp\"]",
            Status = "Scheduled",
            CreatedAt = DateTime.UtcNow
        };

        await context.Appointments.AddAsync(appointment);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Created appointment for {janeSmith.FirstName} (Email + WhatsApp)");

        Console.WriteLine("🎉 Manual seeding completed successfully!");
    }
}
