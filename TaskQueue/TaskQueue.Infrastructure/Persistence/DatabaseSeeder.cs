using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskQueue.Application.DTOs;
using TaskQueue.Domain.Entities;
using TaskQueue.Domain.Enums;

namespace TaskQueue.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly TaskQueueDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(TaskQueueDbContext db, ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await _db.Database.MigrateAsync(ct);

        if (await _db.JobRecords.AnyAsync(ct))
        {
            _logger.LogInformation("Database already seeded, skipping");
            return;
        }

        _logger.LogInformation("Seeding database with sample job records...");

        var faker = new Faker();
        var records = new List<JobRecord>();

        // Seed 20 succeeded notifications
        for (int i = 0; i < 20; i++)
        {
            var payload = new NotificationJobPayload(
                faker.Internet.Email(),
                faker.Name.FullName(),
                faker.Lorem.Sentence(4),
                faker.Lorem.Paragraph()
            );

            var record = JobRecord.Create(
                JobType.Notification,
                JsonSerializer.Serialize(payload),
                correlationId: Guid.NewGuid().ToString());

            record.MarkProcessing($"seed-notif-{i + 1}");
            record.MarkSucceeded();
            records.Add(record);
        }

        // Seed 10 succeeded reports
        var reportTypes = new[] { "inventory", "sales", "audit" };
        for (int i = 0; i < 10; i++)
        {
            var payload = new ReportGenerationJobPayload(
                faker.PickRandom(reportTypes),
                faker.Internet.Email(),
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow
            );

            var record = JobRecord.Create(
                JobType.ReportGeneration,
                JsonSerializer.Serialize(payload),
                correlationId: Guid.NewGuid().ToString());

            record.MarkProcessing($"seed-report-{i + 1}");
            record.MarkSucceeded();
            records.Add(record);
        }

        // Seed 5 dead-lettered jobs
        for (int i = 0; i < 5; i++)
        {
            var payload = new NotificationJobPayload(
                "invalid@nowhere.test",
                faker.Name.FullName(),
                "Failed notification",
                faker.Lorem.Paragraph()
            );

            var record = JobRecord.Create(
                JobType.Notification,
                JsonSerializer.Serialize(payload),
                maxAttempts: 4,
                correlationId: Guid.NewGuid().ToString());

            record.MarkProcessing($"seed-dead-{i + 1}");

            for (int attempt = 0; attempt < 4; attempt++)
                record.MarkFailed("SMTP connection refused", "at System.Net.Mail.SmtpClient.Send()");

            records.Add(record);
        }

        await _db.JobRecords.AddRangeAsync(records, ct);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Seeded {Count} job records", records.Count);
    }
}
