using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskQueue.Domain.Entities;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Hangfire;

/// <summary>
/// Intercepts all Hangfire job state transitions. When a job moves to
/// the Failed state after exhausting retries, writes a JobFailure record
/// to PostgreSQL (our dead-letter audit table).
/// </summary>
public class DeadLetterJobFilter : JobFilterAttribute, IApplyStateFilter
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DeadLetterJobFilter> _logger;

    public DeadLetterJobFilter(IServiceProvider services, ILogger<DeadLetterJobFilter> logger)
    {
        _services = services;
        _logger = logger;
        Order = 1000; // run after Hangfire's built-in filters
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // Only act when a job permanently fails (not transient retry)
        if (context.NewState is not FailedState failedState) return;

        // Check if this is the final failure (no more retries scheduled)
        var retryAttempt = context.Connection
            .GetJobParameter(context.BackgroundJob.Id, "RetryCount");

        _logger.LogError(
            "Job {JobId} permanently failed after {Attempts} attempt(s). Moving to dead-letter. Error: {Error}",
            context.BackgroundJob.Id,
            retryAttempt ?? "unknown",
            failedState.Exception?.Message);

        _ = Task.Run(async () =>
        {
            using var scope = _services.CreateScope();
            var failureRepo = scope.ServiceProvider.GetRequiredService<IJobFailureRepository>();
            var jobRecordRepo = scope.ServiceProvider.GetRequiredService<IJobRecordRepository>();

            try
            {
                var jobRecord = await jobRecordRepo.GetByHangfireIdAsync(context.BackgroundJob.Id);

                var failure = JobFailure.Create(
                    jobRecordId: jobRecord?.Id ?? Guid.Empty,
                    hangfireJobId: context.BackgroundJob.Id,
                    jobType: context.BackgroundJob.Job.Type.Name,
                    payloadJson: System.Text.Json.JsonSerializer.Serialize(
                        context.BackgroundJob.Job.Args),
                    errorMessage: failedState.Exception?.Message ?? "Unknown error",
                    stackTrace: failedState.Exception?.StackTrace,
                    totalAttempts: int.TryParse(retryAttempt, out var attempts) ? attempts + 1 : 1
                );

                await failureRepo.AddAsync(failure);

                if (jobRecord is not null)
                {
                    jobRecord.MarkDeadLettered(failedState.Exception?.Message ?? "Max retries exceeded");
                    await jobRecordRepo.UpdateAsync(jobRecord);
                }

                _logger.LogInformation(
                    "Dead-letter record created for job {JobId} | FailureId: {FailureId}",
                    context.BackgroundJob.Id, failure.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to write dead-letter record for job {JobId}", context.BackgroundJob.Id);
            }
        });
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) { }
}
