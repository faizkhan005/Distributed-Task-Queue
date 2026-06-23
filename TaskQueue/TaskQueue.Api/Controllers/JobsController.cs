using Microsoft.AspNetCore.Mvc;
using TaskQueue.Application.DTOs;
using TaskQueue.Application.Interfaces;
using TaskQueue.Domain.Enums;

namespace TaskQueue.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IJobEnqueueService _enqueue;
    private readonly IJobQueryService _query;

    public JobsController(IJobEnqueueService enqueue, IJobQueryService query)
    {
        _enqueue = enqueue;
        _query = query;
    }

    /// <summary>Enqueue a notification job (email or SMS).</summary>
    [HttpPost("notifications")]
    [ProducesResponseType(typeof(EnqueueJobResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueNotification(
        [FromBody] EnqueueNotificationRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _enqueue.EnqueueNotificationAsync(request, ct);
        return Accepted(result);
    }

    /// <summary>Enqueue a report generation job.</summary>
    [HttpPost("reports")]
    [ProducesResponseType(typeof(EnqueueJobResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueReport(
        [FromBody] EnqueueReportRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _enqueue.EnqueueReportGenerationAsync(request, ct);
        return Accepted(result);
    }

    /// <summary>Enqueue a data sync job.</summary>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(EnqueueJobResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueDataSync(
        [FromBody] EnqueueDataSyncRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _enqueue.EnqueueDataSyncAsync(request, ct);
        return Accepted(result);
    }

    /// <summary>Get status of a specific job by its record ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobStatus(Guid id, CancellationToken ct)
    {
        var result = await _query.GetJobStatusAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List jobs by status (Enqueued, Processing, Succeeded, Failed, DeadLettered, Retrying).</summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IReadOnlyList<JobStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken ct)
    {
        if (!Enum.TryParse<JobStatus>(status, ignoreCase: true, out var parsed))
            return BadRequest($"Invalid status '{status}'. Valid values: {string.Join(", ", Enum.GetNames<JobStatus>())}");

        var results = await _query.GetJobsByStatusAsync(parsed, ct);
        return Ok(results);
    }
}
