using AcsNotification.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcsNotification.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Patient configuration
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.WhatsAppNumber).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Index for performance
            entity.HasIndex(e => e.Email);
        });

        // FollowUp configuration
        modelBuilder.Entity<FollowUp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.EnabledChannels).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FollowUpDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship
            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.FollowUps)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Appointment configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Doctor).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EnabledChannels).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AppointmentDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship
            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // NotificationLog configuration
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Channel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.SentAt).IsRequired();
            entity.Property(e => e.Success).IsRequired();

            // Indexes for querying logs
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.SentAt);
        });
    }
}
