using System;
using System.Collections.Generic;
using System.Text;

namespace TaskQueue.Application.DTOs;

public record JobFailureResponse(
    Guid Id,
    Guid JobRecordId,
    string JobType,
    string ErrorMessage,
    int TotalAttempts,
    DateTime FailedAt,
    bool Requeued,
    DateTime? RequeuedAt
);
