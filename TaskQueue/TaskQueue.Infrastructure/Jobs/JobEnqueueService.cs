using Hangfire;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskQueue.Application.DTOs;
using TaskQueue.Application.Interfaces;
using TaskQueue.Application.Jobs;
using TaskQueue.Domain.Entities;
using TaskQueue.Domain.Enums;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Jobs;

public class JobEnqueueService : IJobEnqueueService
{
    private readonly IBackgroundJobClient _hangfire;
    private readonly IJobRecordRepository _jobRecords;
    private readonly ILogger<JobEnqueueService> _logger;

    public JobEnqueueService(
        IBackgroundJobClient hangfire,
        IJobRecordRepository jobRecords,
        ILogger<JobEnqueueService> logger)
    {
        _hangfire = hangfire;
        _jobRecords = jobRecords;
        _logger = logger;
    }

    public async Task<EnqueueJobResponse> EnqueueNotificationAsync(
        EnqueueNotificationRequest request, CancellationToken ct = default)
    {
        var payload = new NotificationJobPayload(
            request.RecipientEmail, request.RecipientName,
            request.Subject, request.Body, request.Channel);

        var record = JobRecord.Create(
            JobType.Notification,
            JsonSerializer.Serialize(payload),
            maxAttempts: 4,
            queue: "notifications");

        await _jobRecords.AddAsync(record, ct);

        var hangfireId = _hangfire.Enqueue<INotificationJob>(
            "notifications",
            job => job.ExecuteAsync(payload, record.Id.ToString()));
        record.UpdateHangfireJobId(hangfireId);
        await _jobRecords.UpdateAsync(record, ct);

        _logger.LogInformation(
            "Enqueued notification job | JobRecordId: {RecordId} | HangfireId: {HangfireId} | Recipient: {Email}",
            record.Id, hangfireId, request.RecipientEmail);

        return new EnqueueJobResponse(record.Id, hangfireId, "notifications", DateTime.UtcNow);
    }

    public async Task<EnqueueJobResponse> EnqueueReportGenerationAsync(
        EnqueueReportRequest request, CancellationToken ct = default)
    {
        var payload = new ReportGenerationJobPayload(
            request.ReportType, request.RequestedByEmail,
            request.FromDate, request.ToDate, request.OutputFormat);

        var record = JobRecord.Create(
            JobType.ReportGeneration,
            JsonSerializer.Serialize(payload),
            maxAttempts: 3,
            queue: "reports");

        await _jobRecords.AddAsync(record, ct);

        var hangfireId = _hangfire.Enqueue<IReportGenerationJob>(
            "reports",
            job => job.ExecuteAsync(payload, record.Id.ToString()));

        _logger.LogInformation(
            "Enqueued report job | JobRecordId: {RecordId} | HangfireId: {HangfireId} | Type: {ReportType}",
            record.Id, hangfireId, request.ReportType);

        return new EnqueueJobResponse(record.Id, hangfireId, "reports", DateTime.UtcNow);
    }

    public async Task<EnqueueJobResponse> EnqueueDataSyncAsync(
        EnqueueDataSyncRequest request, CancellationToken ct = default)
    {
        var payload = new DataSyncJobPayload(
            request.SourceTable, request.DestinationTable,
            request.BatchSize, request.DryRun);

        var record = JobRecord.Create(
            JobType.DataSync,
            JsonSerializer.Serialize(payload),
            maxAttempts: 3,
            queue: "sync");

        await _jobRecords.AddAsync(record, ct);

        var hangfireId = _hangfire.Enqueue<IDataSyncJob>(
            "sync",
            job => job.ExecuteAsync(payload, record.Id.ToString()));

        _logger.LogInformation(
            "Enqueued data sync job | JobRecordId: {RecordId} | HangfireId: {HangfireId} | {Source} → {Dest}",
            record.Id, hangfireId, request.SourceTable, request.DestinationTable);

        return new EnqueueJobResponse(record.Id, hangfireId, "sync", DateTime.UtcNow);
    }
}
