namespace TaskQueue.Application.Interfaces;

public interface IDeadLetterService
{
    Task<bool> RequeueAsync(Guid failureId, CancellationToken ct = default);
    Task<int> RequeueAllAsync(CancellationToken ct = default);
}
