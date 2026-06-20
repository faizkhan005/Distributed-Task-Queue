using TaskQueue.Application.DTOs;

namespace TaskQueue.Application.Jobs;

public interface IReportGenerationJob
{
    Task ExecuteAsync(ReportGenerationJobPayload payload, string jobRecordId);
}
