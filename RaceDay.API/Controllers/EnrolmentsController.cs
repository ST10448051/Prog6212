
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
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrolmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    private int? CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    /// <summary>
    /// Enrols the current Participant in an event category.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CreateEnrolment(CreateEnrolmentDTO dto)
    {
        var participantId = CurrentUserId();
        if (participantId == null)
            return Unauthorized();

        var category = await _context.EventCategories
            .AsNoTracking()
            .Include(ec => ec.Event)
            .FirstOrDefaultAsync(ec =>
                ec.EventCategoryID == dto.EventCategoryID);

        if (category == null)
            return NotFound(new { message = "Event category not found." });

        if (category.Event.Status != "Open")
            return BadRequest(new
            {
                message = "This event is not open for enrolment."
            });

        if (category.Event.EventDate.Date < DateTime.UtcNow.Date)
            return BadRequest(new
            {
                message = "Cannot enrol in a past event."
            });

        var existing = await _context.Enrolments
            .AnyAsync(e =>
                e.ParticipantID == participantId.Value &&
                e.EventCategoryID == dto.EventCategoryID);

        if (existing)
            return Conflict(new
            {
                message = "You already have an enrolment for this category."
            });

        if (category.MaxParticipants.HasValue)
        {
            var currentCount = await _context.Enrolments
                .CountAsync(e =>
                    e.EventCategoryID == dto.EventCategoryID &&
                    (e.Status == "Pending" ||
                     e.Status == "Confirmed"));

            if (currentCount >= category.MaxParticipants.Value)
                return Conflict(new
                {
                    message = "This event category is full."
                });
        }

        var enrolment = new Enrolment
        {
            ParticipantID = participantId.Value,
            EventCategoryID = dto.EventCategoryID,
            Status = "Confirmed"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEnrolment),
            new { id = enrolment.EnrolmentID },
            new
            {
                message = "Enrolment successful.",
                enrolment.EnrolmentID,
                enrolment.EventCategoryID,
                enrolment.Status
            });
    }

    /// <summary>
    /// Retrieves enrolments belonging to the current Participant.
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var participantId = CurrentUserId();
        if (participantId == null)
            return Unauthorized();

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.ParticipantID == participantId.Value)
            .Select(e => new
            {
                e.EnrolmentID,
                e.EventCategoryID,
                EventName = e.EventCategory.Event.EventName,
                CategoryName = e.EventCategory.Category.CategoryName,
                e.EnrolmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>
    /// Retrieves an enrolment for its Participant or event Organiser.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEnrolment(int id)
    {
        var userId = CurrentUserId();
        if (userId == null)
            return Unauthorized();

        var enrolment = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.EnrolmentID == id)
            .Select(e => new
            {
                e.EnrolmentID,
                e.ParticipantID,
                e.EventCategoryID,
                EventName = e.EventCategory.Event.EventName,
                OrganiserID = e.EventCategory.Event.OrganiserID,
                e.EnrolmentDate,
                e.Status
            })
            .FirstOrDefaultAsync();

        if (enrolment == null)
            return NotFound();

        if (enrolment.ParticipantID != userId.Value &&
            enrolment.OrganiserID != userId.Value)
            return Forbid();

        return Ok(new
        {
            enrolment.EnrolmentID,
            enrolment.ParticipantID,
            enrolment.EventCategoryID,
            enrolment.EventName,
            enrolment.EnrolmentDate,
            enrolment.Status
        });
    }

    /// <summary>
    /// Cancels an enrolment owned by the current Participant.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var participantId = CurrentUserId();
        if (participantId == null)
            return Unauthorized();

        var enrolment = await _context.Enrolments
            .FirstOrDefaultAsync(e =>
                e.EnrolmentID == id &&
                e.ParticipantID == participantId.Value);

        if (enrolment == null)
            return NotFound(new
            {
                message = "Enrolment not found."
            });

        if (enrolment.Status == "Cancelled")
            return Conflict(new
            {
                message = "Enrolment is already cancelled."
            });

        if (enrolment.Status == "Completed")
            return Conflict(new
            {
                message = "Completed enrolments cannot be cancelled."
            });

        enrolment.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Enrolment cancelled successfully."
        });
    }
}
