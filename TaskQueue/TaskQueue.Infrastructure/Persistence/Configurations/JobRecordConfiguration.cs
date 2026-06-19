using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskQueue.Domain.Entities;

namespace TaskQueue.Infrastructure.Persistence.Configurations;

public class JobRecordConfiguration : IEntityTypeConfiguration<JobRecord>
{
    public void Configure(EntityTypeBuilder<JobRecord> builder)
    {
        builder.ToTable("job_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.HangfireJobId)
            .HasColumnName("hangfire_job_id")
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Queue)
            .HasColumnName("queue")
            .HasMaxLength(100)
            .HasDefaultValue("default");

        builder.Property(x => x.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb");

        builder.Property(x => x.AttemptCount)
            .HasColumnName("attempt_count")
            .HasDefaultValue(0);

        builder.Property(x => x.MaxAttempts)
            .HasColumnName("max_attempts")
            .HasDefaultValue(4);

        builder.Property(x => x.LastErrorMessage)
            .HasColumnName("last_error_message");

        builder.Property(x => x.LastErrorStackTrace)
            .HasColumnName("last_error_stack_trace");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at");

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(x => x.DeadLetteredAt)
            .HasColumnName("dead_lettered_at");

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(100);

        builder.HasIndex(x => x.HangfireJobId)
            .HasDatabaseName("ix_job_records_hangfire_job_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_job_records_status");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("ix_job_records_type");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_job_records_created_at");

        builder.HasIndex(x => x.CorrelationId)
            .HasDatabaseName("ix_job_records_correlation_id");
    }
}
