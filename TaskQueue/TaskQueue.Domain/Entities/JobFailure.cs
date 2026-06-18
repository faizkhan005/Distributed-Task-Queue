namespace TaskQueue.Domain.Entities;

public class JobFailure
{
    public Guid Id { get; private set; }
    public Guid JobRecordId { get; private set; }
    public string HangfireJobId { get; private set; } = string.Empty;
    public string JobType { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public string ErrorMessage { get; private set; } = string.Empty;
    public string? StackTrace { get; private set; }
    public int TotalAttempts { get; private set; }
    public DateTime FailedAt { get; private set; }
    public bool RequeuerAt { get; private set; }
    public DateTime? RequeuedAt { get; private set; }

    private JobFailure() { }

    public static JobFailure Create(
        Guid jobRecordId,
        string hangfireJobId,
        string jobType,
        string payloadJson,
        string errorMessage,
        string? stackTrace,
        int totalAttempts)
    {
        return new JobFailure
        {
            Id = Guid.NewGuid(),
            JobRecordId = jobRecordId,
            HangfireJobId = hangfireJobId,
            JobType = jobType,
            PayloadJson = payloadJson,
            ErrorMessage = errorMessage,
            StackTrace = stackTrace,
            TotalAttempts = totalAttempts,
            FailedAt = DateTime.UtcNow
        };
    }

    public void MarkRequeued()
    {
        RequeuerAt = true;
        RequeuedAt = DateTime.UtcNow;
    }

}
