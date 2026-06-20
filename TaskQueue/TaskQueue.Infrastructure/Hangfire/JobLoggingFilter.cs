using Hangfire.Common;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace TaskQueue.Infrastructure.Hangfire;

/// <summary>
/// Wraps every job execution with structured logging including
/// job ID, type, attempt number, and duration.
/// </summary>
public class JobLoggingFilter : JobFilterAttribute, IServerFilter
{
    private readonly ILogger<JobLoggingFilter> _logger;

    public JobLoggingFilter(ILogger<JobLoggingFilter> logger) => _logger = logger;

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
    }

    public void OnPerformed(PerformedContext context)
    {
        var startedAt = context.Items.TryGetValue("StartedAt", out var s) ? (DateTime)s : DateTime.UtcNow;
        var attempt = context.Items.TryGetValue("Attempt", out var a) ? (int)a : 1;
        var elapsed = DateTime.UtcNow - startedAt;

        if (context.Exception is not null && !context.ExceptionHandled)
        {
            _logger.LogError(context.Exception,
                "[HangfireJob] Failed | JobId: {JobId} | Type: {JobType} | Attempt: {Attempt} | Duration: {Elapsed}ms | Error: {Error}",
                context.BackgroundJob.Id,
                context.BackgroundJob.Job.Type.Name,
                attempt,
                elapsed.TotalMilliseconds,
                context.Exception.Message);
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
}

