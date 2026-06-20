using Hangfire;
using Microsoft.Extensions.Logging;
using TaskQueue.Application.DTOs;
using TaskQueue.Application.Jobs;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Jobs;

public class DataSyncJob : IDataSyncJob
{
    private readonly IJobRecordRepository _jobRecords;
    private readonly ILogger<DataSyncJob> _logger;


    public DataSyncJob(IJobRecordRepository jobRecords, ILogger<DataSyncJob> logger)
    {
        _jobRecords = jobRecords;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 180, 900 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    [JobDisplayName("DataSync: {0} → {1}")]
    public async Task ExecuteAsync(DataSyncJobPayload payload, string jobRecordId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobRecordId"] = jobRecordId,
            ["JobType"] = "DataSync",
            ["Source"] = payload.SourceTable,
            ["Destination"] = payload.DestinationTable
        });

        _logger.LogInformation(
            "Starting data sync: {Source} → {Destination} | BatchSize: {BatchSize} | DryRun: {DryRun}",
            payload.SourceTable, payload.DestinationTable, payload.BatchSize, payload.DryRun);

        if (Guid.TryParse(jobRecordId, out var recordId))
        {
            var record = await _jobRecords.GetByIdAsync(recordId);
            if (record is not null)
            {
                record.MarkProcessing(jobRecordId);
                await _jobRecords.UpdateAsync(record);
            }
        }

        // Simulate reading total record count from source
        var totalRecords = Random.Shared.Next(100, 5000);
        var processedBatches = 0;
        var totalBatches = (int)Math.Ceiling((double)totalRecords / payload.BatchSize);
        var upsertedCount = 0;
        var skippedCount = 0;

        _logger.LogInformation("Found {Total} records to sync in {Batches} batches",
            totalRecords, totalBatches);

        // Process in batches — key pattern for idempotency
        for (int batch = 0; batch < totalBatches; batch++)
        {
            var batchSize = Math.Min(payload.BatchSize, totalRecords - (batch * payload.BatchSize));

            // Simulate batch processing latency
            await Task.Delay(TimeSpan.FromMilliseconds(batchSize / 5));

            // Simulate idempotent upsert — some records already exist
            var batchUpserted = (int)(batchSize * 0.75);
            var batchSkipped = batchSize - batchUpserted;
            upsertedCount += batchUpserted;
            skippedCount += batchSkipped;
            processedBatches++;

            if (batch % 5 == 0 || batch == totalBatches - 1)
            {
                _logger.LogInformation(
                    "Batch progress: {Done}/{Total} | Upserted: {Upserted} | Skipped (already current): {Skipped}",
                    processedBatches, totalBatches, upsertedCount, skippedCount);
            }
        }

        if (payload.DryRun)
        {
            _logger.LogInformation(
                "[DRY RUN] Sync simulation complete. Would have upserted {Upserted}, skipped {Skipped}",
                upsertedCount, skippedCount);
        }
        else
        {
            _logger.LogInformation(
                "Data sync completed | Upserted: {Upserted} | Skipped: {Skipped} | Source: {Source} → {Dest}",
                upsertedCount, skippedCount, payload.SourceTable, payload.DestinationTable);
        }

        if (Guid.TryParse(jobRecordId, out recordId))
        {
            var record = await _jobRecords.GetByIdAsync(recordId);
            if (record is not null)
            {
                record.MarkSucceeded();
                await _jobRecords.UpdateAsync(record);
            }
        }
    }
}
