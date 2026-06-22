using Microsoft.Extensions.Logging;
using TaskQueue.Application.Interfaces;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Jobs;

public class DeadLetterService : IDeadLetterService
{
    private readonly IJobFailureRepository _failures;
    private readonly IJobEnqueueService _enqueue;
    private readonly ILogger<DeadLetterService> _logger;

    public DeadLetterService(
        IJobFailureRepository failures,
        IJobEnqueueService enqueue,
        ILogger<DeadLetterService> logger)
    {
        _failures = failures;
        _enqueue = enqueue;
        _logger = logger;
    }

    public async Task<bool> RequeueAsync(Guid failureId, CancellationToken ct = default)
    {
        var failure = await _failures.GetByIdAsync(failureId, ct);
        if (failure is null || failure.RequeuerAt) return false;

        _logger.LogInformation(
            "Requeuing dead-lettered job | FailureId: {FailureId} | Type: {Type}",
            failureId, failure.JobType);

        failure.MarkRequeued();
        await _failures.UpdateAsync(failure, ct);
        return true;
    }

    public async Task<int> RequeueAllAsync(CancellationToken ct = default)
    {
        var failures = await _failures.GetAllAsync(ct);
        var pending = failures.Where(f => !f.RequeuerAt).ToList();

        foreach (var failure in pending)
        {
            failure.MarkRequeued();
            await _failures.UpdateAsync(failure, ct);
        }

        _logger.LogInformation("Requeued {Count} dead-lettered jobs", pending.Count);
        return pending.Count;
    }
}

