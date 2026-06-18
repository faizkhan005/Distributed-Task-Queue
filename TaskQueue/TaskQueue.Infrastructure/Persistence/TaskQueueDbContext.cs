using Microsoft.EntityFrameworkCore;
using TaskQueue.Domain.Entities;

namespace TaskQueue.Infrastructure.Persistence;

public class TaskQueueDbContext : DbContext
{
    public TaskQueueDbContext(DbContextOptions<TaskQueueDbContext> options) : base(options) { }

    public DbSet<JobRecord> JobRecords => Set<JobRecord>();
    public DbSet<JobFailure> JobFailures => Set<JobFailure>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskQueueDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
