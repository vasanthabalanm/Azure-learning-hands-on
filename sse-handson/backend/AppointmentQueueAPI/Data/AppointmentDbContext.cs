using AppointmentQueueAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentQueueAPI.Data;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
        : base(options) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PatientName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("Pending");

            entity.Property(e => e.AssignedDoctorName)
                  .HasMaxLength(100);

            entity.Property(e => e.PickedByDoctorName)
                  .HasMaxLength(100);

            entity.Property(e => e.BookedAt)
                  .HasDefaultValueSql("NOW()");

            // RowVersion maps to xmin in PostgreSQL for optimistic concurrency
            entity.Property(e => e.RowVersion)
                  .IsRowVersion()
                  .IsConcurrencyToken();
        });
    }
}
