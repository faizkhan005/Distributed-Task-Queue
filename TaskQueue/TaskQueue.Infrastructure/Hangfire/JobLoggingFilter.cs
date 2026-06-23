using Hangfire.Common;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Hangfire;

/// <summary>
/// Wraps every job execution with structured logging including
/// job ID, type, attempt number, and duration.
/// </summary>
public class JobLoggingFilter : JobFilterAttribute, IServerFilter
{
    private readonly IServiceProvider _services;
    private readonly ILogger<JobLoggingFilter> _logger;

    public JobLoggingFilter(IServiceProvider services, ILogger<JobLoggingFilter> logger) 
    {
        _services = services;
        _logger = logger;
    }

    public void OnPerforming(PerformingContext context)
    {
        var retryCount = context.GetJobParameter<int>("RetryCount");
        var attempt = retryCount + 1;

        context.Items["StartedAt"] = DateTime.UtcNow;
        context.Items["Attempt"] = attempt;

        _logger.LogInformation(
            "[HangfireJob] Starting | JobId: {JobId} | Type: {JobType} | Attempt: {Attempt}",
            context.BackgroundJob.Id,
            context.BackgroundJob.Job.Type.Name,
            attempt);
        if (retryCount == 0)
        {
            _ = MarkProcessingAsync(context.BackgroundJob.Id);
        }
    }

    public void OnPerformed(PerformedContext context)
    {
        var startedAt = context.Items.TryGetValue("StartedAt", out var s) ? (DateTime)s : DateTime.UtcNow;
        var attempt = context.Items.TryGetValue("Attempt", out var a) ? (int)a : 1;
        var elapsed = DateTime.UtcNow - startedAt;
        var retryCount = context.GetJobParameter<int>("RetryCount");

        if (context.Exception is not null && !context.ExceptionHandled)
        {
            _logger.LogError(context.Exception,
                "[HangfireJob] Failed | JobId: {JobId} | Type: {JobType} | Attempt: {Attempt} | Duration: {Elapsed}ms | Error: {Error}",
                context.BackgroundJob.Id,
                context.BackgroundJob.Job.Type.Name,
                attempt,
                elapsed.TotalMilliseconds,
                context.Exception.Message);

            _ = UpdateJobStatusOnFailureAsync(
                context.BackgroundJob.Id,
                context.Exception,
                retryCount + 1);
        }
        else
        {
            _logger.LogInformation(
                "[HangfireJob] Succeeded | JobId: {JobId} | Type: {JobType} | Attempt: {Attempt} | Duration: {Elapsed}ms",
                context.BackgroundJob.Id,
                context.BackgroundJob.Job.Type.Name,
                attempt,
                elapsed.TotalMilliseconds);
        }
    }

    private async Task UpdateJobStatusOnFailureAsync(
    string hangfireJobId, Exception ex, int attemptCount)
    {
        try
        {
            using var scope = _services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IJobRecordRepository>();

            var record = await repo.GetByHangfireIdAsync(hangfireJobId);
            if (record is null) return;

            record.MarkFailed(ex.Message, ex.StackTrace);
            await repo.UpdateAsync(record);
        }
        catch (Exception innerEx)
        {
            _logger.LogError(innerEx,
                "Failed to update job record status for HangfireJobId: {Id}", hangfireJobId);
        }
    }

    private async Task MarkProcessingAsync(string hangfireJobId)
    {
        try
        {
            using var scope = _services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IJobRecordRepository>();

            var record = await repo.GetByHangfireIdAsync(hangfireJobId);
            if (record is null) return;

            record.MarkProcessing(hangfireJobId);
            await repo.UpdateAsync(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to mark job processing for HangfireJobId: {Id}", hangfireJobId);
        }
    }
}

