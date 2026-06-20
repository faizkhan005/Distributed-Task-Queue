namespace TaskQueue.Application.DTOs;

public record DataSyncJobPayload
(
    string SourceTable,
    string DestinationTable,
    int BatchSize = 500,
    bool DryRun = false
);
