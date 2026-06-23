using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TaskQueue.Application.Interfaces;
using TaskQueue.Application.Jobs;
using TaskQueue.Domain.Interfaces;
using TaskQueue.Infrastructure.Hangfire;
using TaskQueue.Infrastructure.Jobs;
using TaskQueue.Infrastructure.Persistence;
using TaskQueue.Infrastructure.Persistence.Repositories;

namespace TaskQueue.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL + EF Core
        services.AddDbContext<TaskQueueDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(TaskQueueDbContext).Assembly.FullName)));

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        // Repositories
        services.AddScoped<IJobRecordRepository, JobRecordRepository>();
        services.AddScoped<IJobFailureRepository, JobFailureRepository>();

        // Application services
        services.AddScoped<IJobEnqueueService, JobEnqueueService>();
        services.AddScoped<IJobQueryService, JobQueryService>();
        services.AddScoped<IDeadLetterService, DeadLetterService>();

        // Job implementations
        services.AddScoped<INotificationJob, NotificationJob>();
        services.AddScoped<IReportGenerationJob, ReportGenerationJob>();
        services.AddScoped<IDataSyncJob, DataSyncJob>();

        // Seeder
        services.AddScoped<DatabaseSeeder>();

        // Hangfire — use PostgreSQL as persistent storage
        services.AddHangfire((sp, config) =>
        {
            var logger = sp.GetRequiredService<ILogger<DeadLetterJobFilter>>();
            var loggingLogger = sp.GetRequiredService<ILogger<JobLoggingFilter>>();

            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(opts =>
                    opts.UseNpgsqlConnection(configuration.GetConnectionString("Postgres")),
                    new PostgreSqlStorageOptions
                    {
                        PrepareSchemaIfNecessary = true,
                        SchemaName = "hangfire"
                    })
                .UseFilter(new DeadLetterJobFilter(sp, logger))
                .UseFilter(new JobLoggingFilter(loggingLogger));
        });

        // Hangfire server — three named queues with priority order
        services.AddHangfireServer(opts =>
        {
            opts.ServerName = $"taskqueue-worker-{Environment.MachineName}";
            opts.WorkerCount = Environment.ProcessorCount * 2;
            opts.Queues = ["notifications", "reports", "sync", "default"];
            opts.SchedulePollingInterval = TimeSpan.FromSeconds(15);
            opts.ShutdownTimeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}

