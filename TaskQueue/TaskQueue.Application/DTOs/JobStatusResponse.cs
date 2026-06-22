namespace TaskQueue.Application.DTOs;

public record JobStatusResponse(
    Guid Id,
    string HangfireJobId,
    string Type,
    string Status,
    string Queue,
    int AttemptCount,
    int MaxAttempts,
    string? LastError,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime? DeadLetteredAt,
    string? CorrelationId
);
