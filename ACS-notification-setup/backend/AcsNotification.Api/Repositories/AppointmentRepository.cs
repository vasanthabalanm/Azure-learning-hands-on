using AcsNotification.Api.Data;
using AcsNotification.Api.Entities;
using AcsNotification.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcsNotification.Api.Repositories;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Appointment?> GetWithPatientAsync(Guid appointmentId)
    {
        return await _dbSet
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync()
    {
        return await _dbSet
            .Include(a => a.Patient)
            .Where(a => a.Status == "Scheduled" && a.AppointmentDate >= DateTime.UtcNow)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId)
    {
        return await _dbSet
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }
}
