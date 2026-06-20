namespace TaskQueue.Application.DTOs;

public record ReportGenerationJobPayload
(
    string ReportType,        // "inventory" | "sales" | "audit"
    string RequestedByEmail,
    DateTime FromDate,
    DateTime ToDate,
    string OutputFormat = "csv"  // "csv" | "pdf"
);
