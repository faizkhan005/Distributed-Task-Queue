using Hangfire;
using Microsoft.Extensions.Logging;
using TaskQueue.Application.DTOs;
using TaskQueue.Application.Jobs;
using TaskQueue.Domain.Interfaces;

namespace TaskQueue.Infrastructure.Jobs;

public class NotificationJob : INotificationJob
{
    private readonly IJobRecordRepository _jobRecords;
    private readonly ILogger<NotificationJob> _logger;

    // Configurable via env var NOTIFICATION_FAILURE_RATE (0.0 - 1.0)
    private static readonly double FailureRate =
        double.TryParse(Environment.GetEnvironmentVariable("NOTIFICATION_FAILURE_RATE"), out var r) ? r : 1.0;

    public NotificationJob(IJobRecordRepository jobRecords, ILogger<NotificationJob> logger)
    {
        _jobRecords = jobRecords;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 4, DelaysInSeconds = new[] { 30, 120, 600, 3600 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ExecuteAsync(NotificationJobPayload payload, string jobRecordId)
    {
        var correlationId = Guid.TryParse(jobRecordId, out var id) ? jobRecordId : Guid.NewGuid().ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobRecordId"] = jobRecordId,
            ["JobType"] = "Notification",
            ["Recipient"] = payload.RecipientEmail
        });

        _logger.LogInformation("Starting notification job for {Recipient} via {Channel}",
            payload.RecipientEmail, payload.Channel);

        // Update record to processing
        if (Guid.TryParse(jobRecordId, out var recordId))
        {
            var record = await _jobRecords.GetByIdAsync(recordId);
            if (record is not null)
            {
                record.MarkProcessing(record.HangfireJobId);
                await _jobRecords.UpdateAsync(record);
            }
        }

        // Simulate network latency
        await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(100, 500)));

        // Simulate configurable failure rate — exercises retry + dead-letter path
        if (Random.Shared.NextDouble() < FailureRate)
        {
            _logger.LogWarning("Notification delivery failed for {Recipient} — simulated SMTP error",
                payload.RecipientEmail);
            throw new InvalidOperationException(
                $"SMTP connection refused when delivering to {payload.RecipientEmail}");
        }

        // Simulate the actual send (swap for real SMTP/Twilio in production)
        _logger.LogInformation(
            "[SENT] {Channel} to {Recipient} | Subject: {Subject}",
            payload.Channel.ToUpper(), payload.RecipientEmail, payload.Subject);

        // Mark succeeded in our audit trail
        if (Guid.TryParse(jobRecordId, out recordId))
        {
            var record = await _jobRecords.GetByIdAsync(recordId);
            if (record is not null)
            {
                record.MarkSucceeded();
                await _jobRecords.UpdateAsync(record);
            }
        }

        _logger.LogInformation("Notification job completed for {Recipient}", payload.RecipientEmail);
    }
}
