namespace TaskQueue.Application.DTOs;

public record EnqueueJobResponse(
    Guid JobRecordId,
    string HangfireJobId,
    string Queue,
    DateTime EnqueuedAt
);
