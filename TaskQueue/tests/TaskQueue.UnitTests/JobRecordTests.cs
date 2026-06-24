using FluentAssertions;
using TaskQueue.Domain.Entities;
using TaskQueue.Domain.Enums;

namespace TaskQueue.UnitTests;

public class JobRecordTests
{
    [Fact]
    public void Create_ShouldInitializeWithEnqueuedStatus()
    {
        var record = JobRecord.Create(JobType.Notification, "{}", maxAttempts: 4);

        record.Status.Should().Be(JobStatus.Enqueued);
        record.AttemptCount.Should().Be(0);
        record.MaxAttempts.Should().Be(4);
        record.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkProcessing_ShouldUpdateStatusAndHangfireId()
    {
        var record = JobRecord.Create(JobType.Notification, "{}");

        record.MarkProcessing("hangfire-123");

        record.Status.Should().Be(JobStatus.Processing);
        record.HangfireJobId.Should().Be("hangfire-123");
        record.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkSucceeded_ShouldSetSucceededStatus()
    {
        var record = JobRecord.Create(JobType.Notification, "{}");
        record.MarkProcessing("hangfire-123");

        record.MarkSucceeded();

        record.Status.Should().Be(JobStatus.Succeeded);
        record.CompletedAt.Should().NotBeNull();
        record.LastErrorMessage.Should().BeNull();
    }

    [Fact]
    public void MarkFailed_BelowMaxAttempts_ShouldSetRetryingStatus()
    {
        var record = JobRecord.Create(JobType.Notification, "{}", maxAttempts: 4);
        record.MarkProcessing("hangfire-123");

        record.MarkFailed("Connection refused");

        record.Status.Should().Be(JobStatus.Retrying);
        record.AttemptCount.Should().Be(1);
        record.LastErrorMessage.Should().Be("Connection refused");
    }

    [Fact]
    public void MarkFailed_AtMaxAttempts_ShouldSetDeadLetteredStatus()
    {
        var record = JobRecord.Create(JobType.Notification, "{}", maxAttempts: 4);
        record.MarkProcessing("hangfire-1");

        // Fail 4 times — should dead-letter on the 4th
        for (int i = 0; i < 4; i++)
            record.MarkFailed("SMTP error");

        record.Status.Should().Be(JobStatus.DeadLettered);
        record.AttemptCount.Should().Be(4);
        record.DeadLetteredAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkDeadLettered_ShouldSetDeadLetteredStatusAndReason()
    {
        var record = JobRecord.Create(JobType.DataSync, "{}");

        record.MarkDeadLettered("Manual dead-letter");

        record.Status.Should().Be(JobStatus.DeadLettered);
        record.LastErrorMessage.Should().Be("Manual dead-letter");
        record.DeadLetteredAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueCorrelationId_WhenNotProvided()
    {
        var record1 = JobRecord.Create(JobType.Notification, "{}");
        var record2 = JobRecord.Create(JobType.Notification, "{}");

        record1.CorrelationId.Should().NotBeNullOrEmpty();
        record1.CorrelationId.Should().NotBe(record2.CorrelationId);
    }

    [Fact]
    public void Create_ShouldUseProvidedCorrelationId()
    {
        var correlationId = "test-correlation-123";
        var record = JobRecord.Create(JobType.Notification, "{}", correlationId: correlationId);

        record.CorrelationId.Should().Be(correlationId);
    }
}

