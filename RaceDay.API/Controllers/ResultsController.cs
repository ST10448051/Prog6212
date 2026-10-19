
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    private int? CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    private static bool IsValidResult(
        string status, TimeSpan? finishTime, int? position)
    {
        if (finishTime.HasValue &&
            (finishTime.Value <= TimeSpan.Zero ||
             finishTime.Value >= TimeSpan.FromDays(1)))
            return false;

        if (status == "Finished")
            return finishTime.HasValue && position.HasValue;

        return finishTime == null && position == null;
    }

    /// <summary>
    /// Retrieves recorded results for a particular event.
    /// </summary>
    [HttpGet("event/{eventId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        if (!await _context.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var results = await _context.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment.EventCategory.EventID == eventId)
            .OrderBy(r => r.Position)
            .Select(r => new
            {
                r.ResultID,
                r.EnrolmentID,
                ParticipantName =
                    r.Enrolment.Participant.FirstName + " " +
                    r.Enrolment.Participant.LastName,
                EventName =
                    r.Enrolment.EventCategory.Event.EventName,
                CategoryName =
                    r.Enrolment.EventCategory.Category.CategoryName,
                r.FinishTime,
                r.Position,
                r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>
    /// Retrieves results belonging to the logged-in Participant.
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        var userId = CurrentUserId();
        if (userId == null)
            return Unauthorized();

        var results = await _context.Results
            .AsNoTracking()
            .Where(r => r.Enrolment.ParticipantID == userId.Value)
            .Select(r => new
            {
                r.ResultID,
                EventName =
                    r.Enrolment.EventCategory.Event.EventName,
                CategoryName =
                    r.Enrolment.EventCategory.Category.CategoryName,
                r.FinishTime,
                r.Position,
                r.ResultStatus,
                r.RecordedAt
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>
    /// Retrieves a result by its ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetResult(int id)
    {
        var result = await _context.Results
            .AsNoTracking()
            .Where(r => r.ResultID == id)
            .Select(r => new
            {
                r.ResultID,
                r.EnrolmentID,
                EventName =
                    r.Enrolment.EventCategory.Event.EventName,
                CategoryName =
                    r.Enrolment.EventCategory.Category.CategoryName,
                r.FinishTime,
                r.Position,
                r.ResultStatus,
                r.RecordedAt
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return NotFound(new { message = "Result not found." });

        return Ok(result);
    }

    /// <summary>
    /// Records a result for an enrolment.
    /// Only the event's Organiser can record it.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult(CreateResultDTO dto)
    {
        var organiserId = CurrentUserId();
        if (organiserId == null)
            return Unauthorized();

        if (!IsValidResult(dto.ResultStatus, dto.FinishTime, dto.Position))
            return BadRequest(new
            {
                message = "Finished results require a positive finish time " +
                          "and position. Other statuses must have neither."
            });

        var enrolment = await _context.Enrolments
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(e =>
                e.EnrolmentID == dto.EnrolmentID);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        if (enrolment.EventCategory.Event.OrganiserID != organiserId.Value)
            return Forbid();

        if (enrolment.Status == "Cancelled")
            return Conflict(new
            {
                message = "Cannot record results for a cancelled enrolment."
            });

        if (await _context.Results
            .AnyAsync(r => r.EnrolmentID == dto.EnrolmentID))
            return Conflict(new
            {
                message = "This enrolment already has a result."
            });

        var result = new Result
        {
            EnrolmentID = dto.EnrolmentID,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            ResultStatus = dto.ResultStatus
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetResult),
            new { id = result.ResultID },
            new
            {
                result.ResultID,
                result.EnrolmentID,
                result.FinishTime,
                result.Position,
                result.ResultStatus
            });
    }

    /// <summary>
    /// Updates an existing result.
    /// Only the event's Organiser can modify it.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateResult(
        int id, UpdateResultDTO dto)
    {
        var organiserId = CurrentUserId();
        if (organiserId == null)
            return Unauthorized();

        if (!IsValidResult(dto.ResultStatus, dto.FinishTime, dto.Position))
            return BadRequest(new
            {
                message = "Invalid finish time, position or result status."
            });

        var result = await _context.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.EventCategory)
            .ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(r => r.ResultID == id);

        if (result == null)
            return NotFound(new { message = "Result not found." });

        if (result.Enrolment.EventCategory.Event.OrganiserID != organiserId.Value)
            return Forbid();

        result.FinishTime = dto.FinishTime;
        result.Position = dto.Position;
        result.ResultStatus = dto.ResultStatus;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Result updated successfully." });
    }

    /// <summary>
    /// Deletes a result belonging to the Organiser's event.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteResult(int id)
    {
        var organiserId = CurrentUserId();
        if (organiserId == null)
            return Unauthorized();

        var result = await _context.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.EventCategory)
            .ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(r => r.ResultID == id);

        if (result == null)
            return NotFound(new { message = "Result not found." });

        if (result.Enrolment.EventCategory.Event.OrganiserID != organiserId.Value)
            return Forbid();

        _context.Results.Remove(result);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
