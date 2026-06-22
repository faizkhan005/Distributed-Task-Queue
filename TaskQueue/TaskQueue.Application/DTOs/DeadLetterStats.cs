namespace TaskQueue.Application.DTOs;

public record DeadLetterStats(
    int TotalDeadLettered,
    int TotalRequeued,
    int PendingReview,
    IReadOnlyList<JobFailureResponse> Failures
);