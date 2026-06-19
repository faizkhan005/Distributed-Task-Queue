using Microsoft.EntityFrameworkCore;
using TaskQueue.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<TaskQueueDbContext>(opts =>
            opts.UseNpgsql(
                builder.Configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(TaskQueueDbContext).Assembly.FullName)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
