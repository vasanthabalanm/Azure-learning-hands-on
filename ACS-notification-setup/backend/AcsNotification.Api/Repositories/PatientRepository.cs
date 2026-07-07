using AcsNotification.Api.Data;
using AcsNotification.Api.Entities;
using AcsNotification.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcsNotification.Api.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Patient?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<Patient?> GetWithFollowUpsAsync(Guid patientId)
    {
        return await _dbSet
            .Include(p => p.FollowUps)
            .FirstOrDefaultAsync(p => p.Id == patientId);
    }

    public async Task<Patient?> GetWithAppointmentsAsync(Guid patientId)
    {
        return await _dbSet
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == patientId);
    }
}
