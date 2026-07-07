using AcsNotification.Api.Data;
using AcsNotification.Api.Entities;
using AcsNotification.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcsNotification.Api.Repositories;

public class FollowUpRepository : Repository<FollowUp>, IFollowUpRepository
{
    public FollowUpRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<FollowUp?> GetWithPatientAsync(Guid followUpId)
    {
        return await _dbSet
            .Include(f => f.Patient)
            .FirstOrDefaultAsync(f => f.Id == followUpId);
    }

    public async Task<IEnumerable<FollowUp>> GetPendingFollowUpsAsync()
    {
        return await _dbSet
            .Include(f => f.Patient)
            .Where(f => f.Status == "Pending")
            .OrderBy(f => f.FollowUpDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<FollowUp>> GetByPatientIdAsync(Guid patientId)
    {
        return await _dbSet
            .Where(f => f.PatientId == patientId)
            .OrderByDescending(f => f.FollowUpDate)
            .ToListAsync();
    }
}
