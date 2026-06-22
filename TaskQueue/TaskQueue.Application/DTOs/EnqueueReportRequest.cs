namespace TaskQueue.Application.DTOs;

public record EnqueueReportRequest
(
    string ReportType,
    string RequestedByEmail,
    DateTime FromDate,
    DateTime ToDate,
    string OutputFormat = "csv"
);
