using Microsoft.EntityFrameworkCore;
using TaskQueue.Domain.Entities;
using TaskQueue.Domain.Enums;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Persistence.Repositories;

public class JobRecordRepository : IJobRecordRepository
{

    private readonly TaskQueueDbContext _db;

    public JobRecordRepository(TaskQueueDbContext db) => _db = db;

    public async Task<JobRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.JobRecords.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<JobRecord?> GetByHangfireIdAsync(string hangfireJobId, CancellationToken ct = default)
        => await _db.JobRecords.FirstOrDefaultAsync(x => x.HangfireJobId == hangfireJobId, ct);

    public async Task<IReadOnlyList<JobRecord>> GetByStatusAsync(JobStatus status, CancellationToken ct = default)
        => await _db.JobRecords
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(JobRecord record, CancellationToken ct = default)
    {
        await _db.JobRecords.AddAsync(record, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(JobRecord record, CancellationToken ct = default)
    {
        _db.JobRecords.Update(record);
        await _db.SaveChangesAsync(ct);
    }
}
