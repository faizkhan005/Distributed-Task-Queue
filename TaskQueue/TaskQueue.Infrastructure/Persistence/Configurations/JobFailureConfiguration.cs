using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskQueue.Domain.Entities;

namespace TaskQueue.Infrastructure.Persistence.Configurations;

public class JobFailureConfiguration : IEntityTypeConfiguration<JobFailure>
{
    public void Configure(EntityTypeBuilder<JobFailure> builder)
    {
        builder.ToTable("job_failures");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.JobRecordId)
            .HasColumnName("job_record_id");

        builder.Property(x => x.HangfireJobId)
            .HasColumnName("hangfire_job_id")
            .HasMaxLength(100);

        builder.Property(x => x.JobType)
            .HasColumnName("job_type")
            .HasMaxLength(100);

        builder.Property(x => x.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb");

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(x => x.StackTrace)
            .HasColumnName("stack_trace");

        builder.Property(x => x.TotalAttempts)
            .HasColumnName("total_attempts");

        builder.Property(x => x.FailedAt)
            .HasColumnName("failed_at");

        builder.Property(x => x.RequeuerAt)
            .HasColumnName("requeued");

        builder.Property(x => x.RequeuedAt)
            .HasColumnName("requeued_at");

        builder.HasIndex(x => x.JobRecordId)
            .HasDatabaseName("ix_job_failures_job_record_id");

        builder.HasIndex(x => x.FailedAt)
            .HasDatabaseName("ix_job_failures_failed_at");

        builder.HasIndex(x => x.RequeuerAt)
            .HasDatabaseName("ix_job_failures_requeued");
    }
}
