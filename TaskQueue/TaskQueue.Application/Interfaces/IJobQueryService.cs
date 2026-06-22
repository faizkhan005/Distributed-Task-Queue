using TaskQueue.Application.DTOs;
using TaskQueue.Domain.Enums;

namespace TaskQueue.Application.Interfaces;

public interface IJobQueryService
{
    Task<JobStatusResponse?> GetJobStatusAsync(Guid jobRecordId, CancellationToken ct = default);
    Task<IReadOnlyList<JobStatusResponse>> GetJobsByStatusAsync(JobStatus status, CancellationToken ct = default);
    Task<DeadLetterStats> GetDeadLetterStatsAsync(CancellationToken ct = default);
}
