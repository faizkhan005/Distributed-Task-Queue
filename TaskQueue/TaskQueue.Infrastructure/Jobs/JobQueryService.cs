using TaskQueue.Application.DTOs;
using TaskQueue.Application.Interfaces;
using TaskQueue.Domain.Enums;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Jobs;

public class JobQueryService : IJobQueryService
{
    private readonly IJobRecordRepository _jobRecords;
    private readonly IJobFailureRepository _jobFailures;

    public JobQueryService(IJobRecordRepository jobRecords, IJobFailureRepository jobFailures)
    {
        _jobRecords = jobRecords;
        _jobFailures = jobFailures;
    }

    public async Task<JobStatusResponse?> GetJobStatusAsync(Guid jobRecordId, CancellationToken ct = default)
    {
        var record = await _jobRecords.GetByIdAsync(jobRecordId, ct);
        return record is null ? null : MapToResponse(record);
    }

    public async Task<IReadOnlyList<JobStatusResponse>> GetJobsByStatusAsync(
        JobStatus status, CancellationToken ct = default)
    {
        var records = await _jobRecords.GetByStatusAsync(status, ct);
        return records.Select(MapToResponse).ToList();
    }

    public async Task<DeadLetterStats> GetDeadLetterStatsAsync(CancellationToken ct = default)
    {
        var failures = await _jobFailures.GetAllAsync(ct);
        var responses = failures.Select(f => new JobFailureResponse(
            f.Id, f.JobRecordId, f.JobType, f.ErrorMessage,
            f.TotalAttempts, f.FailedAt, f.RequeuerAt, f.RequeuedAt)).ToList();

        return new DeadLetterStats(
            responses.Count,
            responses.Count(f => f.Requeued),
            responses.Count(f => !f.Requeued),
            responses);
    }

    private static JobStatusResponse MapToResponse(Domain.Entities.JobRecord r) => new(
        r.Id, r.HangfireJobId, r.Type.ToString(), r.Status.ToString(),
        r.Queue, r.AttemptCount, r.MaxAttempts, r.LastErrorMessage,
        r.CreatedAt, r.StartedAt, r.CompletedAt, r.DeadLetteredAt, r.CorrelationId);
}
