using TaskQueue.Domain.Entities;
using TaskQueue.Domain.Enums;

namespace TaskQueue.Domain.Interfaces;

public interface IJobRecordRepository
{
    Task<JobRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<JobRecord?> GetByHangfireIdAsync(string hangfireJobId, CancellationToken ct = default);
    Task<IReadOnlyList<JobRecord>> GetByStatusAsync(JobStatus status, CancellationToken ct = default);
    Task AddAsync(JobRecord record, CancellationToken ct = default);
    Task UpdateAsync(JobRecord record, CancellationToken ct = default);
}
