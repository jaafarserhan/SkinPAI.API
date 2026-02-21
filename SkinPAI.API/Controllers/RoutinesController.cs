using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;
using System.Security.Claims;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoutinesController : ControllerBase
{
    private readonly IRoutineService _routineService;
    private readonly ILogger<RoutinesController> _logger;

    public RoutinesController(IRoutineService routineService, ILogger<RoutinesController> logger)
    {
        _routineService = routineService;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get all user routines
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoutineDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoutineDto>>> GetRoutines()
    {
        var routines = await _routineService.GetUserRoutinesAsync(GetUserId());
        return Ok(routines);
    }

    /// <summary>
    /// Get routine by ID
    /// </summary>
    [HttpGet("{routineId}")]
    [ProducesResponseType(typeof(RoutineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoutineDto>> GetRoutine(Guid routineId)
    {
        var routine = await _routineService.GetRoutineByIdAsync(routineId, GetUserId());
        if (routine == null) return NotFound();
        return Ok(routine);
    }

    /// <summary>
    /// Create a new routine
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoutineDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoutineDto>> CreateRoutine([FromBody] CreateRoutineRequest request)
    {
        var routine = await _routineService.CreateRoutineAsync(GetUserId(), request);
        return Ok(routine);
    }

    /// <summary>
    /// Update a routine
    /// </summary>
    [HttpPut("{routineId}")]
    [ProducesResponseType(typeof(RoutineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoutineDto>> UpdateRoutine(Guid routineId, [FromBody] UpdateRoutineRequest request)
    {
        try
        {
            var routine = await _routineService.UpdateRoutineAsync(routineId, GetUserId(), request);
            return Ok(routine);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete a routine
    /// </summary>
    [HttpDelete("{routineId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRoutine(Guid routineId)
    {
        var deleted = await _routineService.DeleteRoutineAsync(routineId, GetUserId());
        if (!deleted) return NotFound();
        return Ok(new { message = "Routine deleted" });
    }

    /// <summary>
    /// Add a step to routine
    /// </summary>
    [HttpPost("{routineId}/steps")]
    [ProducesResponseType(typeof(RoutineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoutineDto>> AddStep(Guid routineId, [FromBody] CreateRoutineStepRequest request)
    {
        try
        {
            var routine = await _routineService.AddStepAsync(routineId, GetUserId(), request);
            return Ok(routine);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Remove a step from routine
    /// </summary>
    [HttpDelete("{routineId}/steps/{stepId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveStep(Guid routineId, long stepId)
    {
        var removed = await _routineService.RemoveStepAsync(routineId, stepId, GetUserId());
        if (!removed) return NotFound();
        return Ok(new { message = "Step removed" });
    }

    /// <summary>
    /// Complete a routine
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(RoutineCompletionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoutineCompletionDto>> CompleteRoutine([FromBody] CompleteRoutineRequest request)
    {
        try
        {
            var completion = await _routineService.CompleteRoutineAsync(GetUserId(), request);
            return Ok(completion);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get routine completion history
    /// </summary>
    [HttpGet("completions")]
    [ProducesResponseType(typeof(List<RoutineCompletionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoutineCompletionDto>>> GetCompletionHistory([FromQuery] int days = 30)
    {
        var completions = await _routineService.GetCompletionHistoryAsync(GetUserId(), days);
        return Ok(completions);
    }

    // ==================== Reminders ====================

    /// <summary>
    /// Get all reminders
    /// </summary>
    [HttpGet("reminders")]
    [ProducesResponseType(typeof(List<RoutineReminderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoutineReminderDto>>> GetReminders()
    {
        var reminders = await _routineService.GetRemindersAsync(GetUserId());
        return Ok(reminders);
    }

    /// <summary>
    /// Create a reminder
    /// </summary>
    [HttpPost("reminders")]
    [ProducesResponseType(typeof(RoutineReminderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoutineReminderDto>> CreateReminder([FromBody] CreateReminderRequest request)
    {
        try
        {
            var reminder = await _routineService.CreateReminderAsync(GetUserId(), request);
            return Ok(reminder);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Update a reminder
    /// </summary>
    [HttpPut("reminders/{reminderId}")]
    [ProducesResponseType(typeof(RoutineReminderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoutineReminderDto>> UpdateReminder(Guid reminderId, [FromBody] UpdateReminderRequest request)
    {
        try
        {
            var reminder = await _routineService.UpdateReminderAsync(reminderId, GetUserId(), request);
            return Ok(reminder);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete a reminder
    /// </summary>
    [HttpDelete("reminders/{reminderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteReminder(Guid reminderId)
    {
        var deleted = await _routineService.DeleteReminderAsync(reminderId, GetUserId());
        if (!deleted) return NotFound();
        return Ok(new { message = "Reminder deleted" });
    }
}
