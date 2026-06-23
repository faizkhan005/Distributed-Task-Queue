using Hangfire;
using TaskQueue.Application.DTOs;

namespace TaskQueue.Application.Jobs;

public interface INotificationJob
{
    [AutomaticRetry(Attempts = 4, DelaysInSeconds = new[] { 30, 120, 600, 3600 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ExecuteAsync(NotificationJobPayload payload, string jobRecordId);
}
