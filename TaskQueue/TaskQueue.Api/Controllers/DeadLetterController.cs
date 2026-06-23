using Microsoft.AspNetCore.Mvc;
using TaskQueue.Application.DTOs;
using TaskQueue.Application.Interfaces;

namespace TaskQueue.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class DeadLetterController : ControllerBase
{
    private readonly IJobQueryService _query;
    private readonly IDeadLetterService _deadLetter;

    public DeadLetterController(IJobQueryService query, IDeadLetterService deadLetter)
    {
        _query = query;
        _deadLetter = deadLetter;
    }

    /// <summary>Get all dead-lettered jobs with stats.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DeadLetterStats), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeadLetterStats(CancellationToken ct)
    {
        var stats = await _query.GetDeadLetterStatsAsync(ct);
        return Ok(stats);
    }

    /// <summary>Requeue a single dead-lettered job by failure ID.</summary>
    [HttpPost("{id:guid}/requeue")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Requeue(Guid id, CancellationToken ct)
    {
        var success = await _deadLetter.RequeueAsync(id, ct);
        if (!success) return NotFound();
        return NoContent();
    }

    /// <summary>Requeue all pending dead-lettered jobs.</summary>
    [HttpPost("requeue-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequeueAll(CancellationToken ct)
    {
        var count = await _deadLetter.RequeueAllAsync(ct);
        return Ok(new { requeued = count, timestamp = DateTime.UtcNow });
    }

}
