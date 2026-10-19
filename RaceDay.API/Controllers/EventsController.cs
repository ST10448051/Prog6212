
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all RaceDay events.
    /// </summary>
    /// <returns>A list of available race events.</returns>
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.Description,
                e.EventDate,
                e.Location,
                e.DistanceKm,
                e.EntryFee,
                e.Status
            })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>
    /// Retrieves a race event by its ID.
    /// </summary>
    /// <param name="id">The ID of the event.</param>
    /// <returns>The event, or 404 if it does not exist.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var raceEvent = await _context.Events
            .AsNoTracking()
            .Where(e => e.EventID == id)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.Description,
                e.EventDate,
                e.Location,
                e.DistanceKm,
                e.EntryFee,
                e.Status
            })
            .FirstOrDefaultAsync();

        if (raceEvent == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        return Ok(raceEvent);
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        CreateEventDTO dto)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
            return Unauthorized();

        var allowedStatuses = new[]
        {
        "Draft", "Upcoming", "Open",
        "Closed", "Completed", "Cancelled"
    };

        if (!allowedStatuses.Contains(dto.Status))
            return BadRequest(new
            {
                message = "Invalid event status."
            });

        var raceEvent = new RaceDay.API.Models.Event
        {
            OrganiserID = organiserId,
            EventName = dto.EventName,
            Description = dto.Description,
            EventDate = dto.EventDate!.Value,
            Location = dto.Location,
            DistanceKm = dto.DistanceKm,
            EntryFee = dto.EntryFee,
            Status = dto.Status
        };

        _context.Events.Add(raceEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = raceEvent.EventID },
            new
            {
                message = "Event created successfully.",
                raceEvent.EventID
            });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id, UpdateEventDTO dto)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
            return Unauthorized();

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.EventID == id &&
                e.OrganiserID == organiserId);

        if (raceEvent == null)
            return NotFound(new
            {
                message = "Event not found or access denied."
            });

        var allowedStatuses = new[]
        {
        "Draft", "Upcoming", "Open",
        "Closed", "Completed", "Cancelled"
    };

        if (!allowedStatuses.Contains(dto.Status))
            return BadRequest(new
            {
                message = "Invalid event status."
            });

        raceEvent.EventName = dto.EventName;
        raceEvent.Description = dto.Description;
        raceEvent.EventDate = dto.EventDate!.Value;
        raceEvent.Location = dto.Location;
        raceEvent.DistanceKm = dto.DistanceKm;
        raceEvent.EntryFee = dto.EntryFee;
        raceEvent.Status = dto.Status;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Event updated successfully."
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
            return Unauthorized();

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.EventID == id &&
                e.OrganiserID == organiserId);

        if (raceEvent == null)
            return NotFound(new
            {
                message = "Event not found or access denied."
            });

        _context.Events.Remove(raceEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }



}
