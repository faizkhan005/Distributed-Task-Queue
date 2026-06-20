using TaskQueue.Application.DTOs;

namespace TaskQueue.Application.Jobs;

public interface IDataSyncJob
{
    Task ExecuteAsync(DataSyncJobPayload payload, string jobRecordId);
}
