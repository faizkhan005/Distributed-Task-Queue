using Hangfire;
using Microsoft.Extensions.Logging;
using TaskQueue.Application.DTOs;
using TaskQueue.Application.Jobs;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Jobs;

public class ReportGenerationJob : IReportGenerationJob
{
    private readonly IJobRecordRepository _jobRecords;
    private readonly ILogger<ReportGenerationJob> _logger;

    public ReportGenerationJob(IJobRecordRepository jobRecords, ILogger<ReportGenerationJob> logger)
    {
        _jobRecords = jobRecords;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    [JobDisplayName("Report: {0}")]
    public async Task ExecuteAsync(ReportGenerationJobPayload payload, string jobRecordId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobRecordId"] = jobRecordId,
            ["JobType"] = "ReportGeneration",
            ["ReportType"] = payload.ReportType
        });

        _logger.LogInformation(
            "Starting {ReportType} report generation for {Email} | Range: {From:yyyy-MM-dd} to {To:yyyy-MM-dd} | Format: {Format}",
            payload.ReportType, payload.RequestedByEmail,
            payload.FromDate, payload.ToDate, payload.OutputFormat);

        if (Guid.TryParse(jobRecordId, out var recordId))
        {
            var record = await _jobRecords.GetByIdAsync(recordId);
            if (record is not null)
            {
                record.MarkProcessing(jobRecordId);
                await _jobRecords.UpdateAsync(record);
            }
        }

        // Phase 1: data aggregation (simulated)
        _logger.LogInformation("[{ReportType}] Phase 1/3: Aggregating data...", payload.ReportType);
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(1, 3)));

        // Phase 2: formatting
        _logger.LogInformation("[{ReportType}] Phase 2/3: Formatting as {Format}...",
            payload.ReportType, payload.OutputFormat);
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(1, 2)));

        // Phase 3: delivery simulation
        _logger.LogInformation("[{ReportType}] Phase 3/3: Delivering report to {Email}...",
            payload.ReportType, payload.RequestedByEmail);
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        var outputPath = $"/reports/{payload.ReportType}-{DateTime.UtcNow:yyyyMMddHHmmss}.{payload.OutputFormat}";

        if (Guid.TryParse(jobRecordId, out recordId))
        {
            var record = await _jobRecords.GetByIdAsync(recordId);
            if (record is not null)
            {
                record.MarkSucceeded();
                await _jobRecords.UpdateAsync(record);
            }
        }

        _logger.LogInformation(
            "Report generation completed | Type: {ReportType} | Output: {Path}",
            payload.ReportType, outputPath);
    }
}
