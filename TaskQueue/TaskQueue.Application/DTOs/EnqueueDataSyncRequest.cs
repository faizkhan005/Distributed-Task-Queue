namespace TaskQueue.Application.DTOs;

public record EnqueueDataSyncRequest(
    string SourceTable,
    string DestinationTable,
    int BatchSize = 500,
    bool DryRun = false
);
