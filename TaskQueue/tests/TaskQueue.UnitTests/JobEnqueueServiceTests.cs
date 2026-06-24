using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Moq;
using TaskQueue.Application.DTOs;
using TaskQueue.Domain.Entities;
using TaskQueue.Domain.Interfaces;
using TaskQueue.Infrastructure.Jobs;

namespace TaskQueue.UnitTests;

public class JobEnqueueServiceTests
{
    private readonly Mock<IBackgroundJobClient> _hangfire = new();
    private readonly Mock<IJobRecordRepository> _jobRecords = new();
    private readonly Mock<ILogger<JobEnqueueService>> _logger = new();

    private JobEnqueueService CreateSut() =>
        new(_hangfire.Object, _jobRecords.Object, _logger.Object);

    [Fact]
    public async Task EnqueueNotificationAsync_ShouldPersistJobRecordAndReturnResponse()
    {
        _hangfire.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Returns("hangfire-001");

        _jobRecords.Setup(x => x.AddAsync(It.IsAny<JobRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var request = new EnqueueNotificationRequest(
            "test@example.com", "Test User", "Hello", "Body", "email");

        var result = await sut.EnqueueNotificationAsync(request);

        result.Should().NotBeNull();
        result.Queue.Should().Be("notifications");
        result.EnqueuedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _jobRecords.Verify(x =>
            x.AddAsync(It.Is<JobRecord>(r =>
                r.Queue == "notifications" &&
                r.MaxAttempts == 4),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnqueueReportGenerationAsync_ShouldUseReportsQueue()
    {
        _hangfire.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Returns("hangfire-002");

        _jobRecords.Setup(x => x.AddAsync(It.IsAny<JobRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var request = new EnqueueReportRequest(
            "inventory", "admin@example.com",
            DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        var result = await sut.EnqueueReportGenerationAsync(request);

        result.Queue.Should().Be("reports");
    }

    [Fact]
    public async Task EnqueueDataSyncAsync_ShouldUseSyncQueue()
    {
        _hangfire.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Returns("hangfire-003");

        _jobRecords.Setup(x => x.AddAsync(It.IsAny<JobRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var request = new EnqueueDataSyncRequest("products", "products_archive", 500, false);

        var result = await sut.EnqueueDataSyncAsync(request);

        result.Queue.Should().Be("sync");
    }
}

