using Microsoft.EntityFrameworkCore;
using TaskQueue.Domain.Entities;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Persistence.Repositories;

public class JobFailureRepository : IJobFailureRepository
{
    private readonly TaskQueueDbContext _db;

    public JobFailureRepository(TaskQueueDbContext db) => _db = db;

    public async Task<IReadOnlyList<JobFailure>> GetAllAsync(CancellationToken ct = default)
        => await _db.JobFailures
            .OrderByDescending(x => x.FailedAt)
            .ToListAsync(ct);

    public async Task<JobFailure?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.JobFailures.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(JobFailure failure, CancellationToken ct = default)
    {
        await _db.JobFailures.AddAsync(failure, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(JobFailure failure, CancellationToken ct = default)
    {
        _db.JobFailures.Update(failure);
        await _db.SaveChangesAsync(ct);
    }
}
