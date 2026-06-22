using TaskQueue.Application.DTOs;

namespace TaskQueue.Application.Interfaces;

public interface IJobEnqueueService
{
    Task<EnqueueJobResponse> EnqueueNotificationAsync(
       EnqueueNotificationRequest request,
       CancellationToken ct = default);

    Task<EnqueueJobResponse> EnqueueReportGenerationAsync(
        EnqueueReportRequest request,
        CancellationToken ct = default);

    Task<EnqueueJobResponse> EnqueueDataSyncAsync(
        EnqueueDataSyncRequest request,
        CancellationToken ct = default);
}
