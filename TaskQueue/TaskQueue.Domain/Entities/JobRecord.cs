using TaskQueue.Domain.Enums;

namespace TaskQueue.Domain.Entities;

public class JobRecord
{
    public Guid Id { get; private set; }
    public string HangfireJobId { get; private set; } = string.Empty;
    public JobType Type { get; private set; }
    public JobStatus Status { get; private set; }
    public string Queue { get; private set; } = "default";
    public string PayloadJson { get; private set; } = string.Empty;
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; }
    public string? LastErrorMessage { get; private set; }
    public string? LastErrorStackTrace { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? DeadLetteredAt { get; private set; }
    public string? CorrelationId { get; private set; }

    public static JobRecord Create(
       JobType type,
       string payloadJson,
       int maxAttempts = 4,
       string queue = "default",
       string? correlationId = null)
    {
        return new JobRecord
        {
            Id = Guid.NewGuid(),
            Type = type,
            Status = JobStatus.Enqueued,
            PayloadJson = payloadJson,
            MaxAttempts = maxAttempts,
            Queue = queue,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateHangfireJobId(string hangfireJobId)
    {
        HangfireJobId = hangfireJobId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkProcessing(string hangfireJobId)
    {
        HangfireJobId = hangfireJobId;
        Status = JobStatus.Processing;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSucceeded()
    {
        Status = JobStatus.Succeeded;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        LastErrorMessage = null;
        LastErrorStackTrace = null;
    }

    public void MarkFailed(string errorMessage, string? stackTrace = null)
    {
        AttemptCount++;
        Status = AttemptCount >= MaxAttempts ? JobStatus.DeadLettered : JobStatus.Retrying;
        LastErrorMessage = errorMessage;
        LastErrorStackTrace = stackTrace;
        UpdatedAt = DateTime.UtcNow;

        if (Status == JobStatus.DeadLettered)
            DeadLetteredAt = DateTime.UtcNow;
    }

    public void MarkDeadLettered(string reason)
    {
        Status = JobStatus.DeadLettered;
        LastErrorMessage = reason;
        DeadLetteredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
