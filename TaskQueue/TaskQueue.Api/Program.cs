using Hangfire;
using Scalar.AspNetCore;
using TaskQueue.Api.Filters;
using TaskQueue.Api.Middleware;
using TaskQueue.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opts =>
    {
        opts.Title = "Task Queue API";
        opts.Theme = ScalarTheme.Purple;
    });
}

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireDashboardAuthFilter()],
    AppPath = "/",
    DashboardTitle = "Task Queue — Job Dashboard",
    StatsPollingInterval = 5000
});

app.MapControllers();

app.Run();
