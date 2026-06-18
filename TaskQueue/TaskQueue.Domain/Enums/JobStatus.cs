namespace TaskQueue.Domain.Enums;

public enum JobStatus
{
    Enqueued = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    DeadLettered = 4,
    Retrying = 5
}
