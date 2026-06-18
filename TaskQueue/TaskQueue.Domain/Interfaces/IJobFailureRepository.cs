using TaskQueue.Domain.Entities;

namespace TaskQueue.Domain.Interfaces;

public interface IJobFailureRepository
{
    Task<IReadOnlyList<JobFailure>> GetAllAsync(CancellationToken ct = default);
    Task<JobFailure?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(JobFailure failure, CancellationToken ct = default);
    Task UpdateAsync(JobFailure failure, CancellationToken ct = default);
}
