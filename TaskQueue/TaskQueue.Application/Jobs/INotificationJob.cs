using TaskQueue.Application.DTOs;

namespace TaskQueue.Application.Jobs;

public interface INotificationJob
{
    Task ExecuteAsync(NotificationJobPayload payload, string jobRecordId);
}
