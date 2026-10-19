
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
public class EventCategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventCategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the categories available for a specific event.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventCategories(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == eventId);

        if (!eventExists)
            return NotFound(new
            {
                message = "Event not found."
            });

        var categories = await _context.EventCategories
            .AsNoTracking()
            .Where(ec => ec.EventID == eventId)
            .Select(ec => new
            {
                ec.EventCategoryID,
                ec.EventID,
                ec.CategoryID,
                ec.Category.CategoryName,
                ec.Category.Description,
                ec.EntryFee,
                ec.MaxParticipants
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Assigns a category to an event.
    /// Only the event's Organiser can do this.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> AddEventCategory(
        int eventId,
        CreateEventCategoryDTO dto)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
            return Unauthorized();

        var raceEvent = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventID == eventId);

        if (raceEvent == null)
            return NotFound(new
            {
                message = "Event not found."
            });

        if (raceEvent.OrganiserID != organiserId)
            return Forbid();

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.CategoryID == dto.CategoryID);

        if (!categoryExists)
            return NotFound(new
            {
                message = "Category not found."
            });

        var alreadyAssigned = await _context.EventCategories
            .AnyAsync(ec =>
                ec.EventID == eventId &&
                ec.CategoryID == dto.CategoryID);

        if (alreadyAssigned)
            return Conflict(new
            {
                message = "This category is already assigned to the event."
            });

        var eventCategory = new EventCategory
        {
            EventID = eventId,
            CategoryID = dto.CategoryID,
            EntryFee = dto.EntryFee,
            MaxParticipants = dto.MaxParticipants
        };

        _context.EventCategories.Add(eventCategory);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEventCategories),
            new { eventId },
            new
            {
                message = "Category assigned successfully.",
                eventCategory.EventCategoryID,
                eventCategory.EventID,
                eventCategory.CategoryID,
                eventCategory.EntryFee,
                eventCategory.MaxParticipants
            });
    }

    /// <summary>
    /// Updates the fee and participant limit for an event category.
    /// </summary>
    [HttpPut("{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEventCategory(
        int eventId,
        int categoryId,
        UpdateEventCategoryDTO dto)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
            return Unauthorized();

        var eventCategory = await _context.EventCategories
            .Include(ec => ec.Event)
            .FirstOrDefaultAsync(ec =>
                ec.EventID == eventId &&
                ec.CategoryID == categoryId);

        if (eventCategory == null)
            return NotFound(new
            {
                message = "Event category not found."
            });

        if (eventCategory.Event.OrganiserID != organiserId)
            return Forbid();

        eventCategory.EntryFee = dto.EntryFee;
        eventCategory.MaxParticipants = dto.MaxParticipants;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Event category updated successfully."
        });
    }

    /// <summary>
    /// Removes a category from an event if no participants
    /// are enrolled in that category.
    /// </summary>
    [HttpDelete("{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> RemoveEventCategory(
        int eventId,
        int categoryId)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
            return Unauthorized();

        var eventCategory = await _context.EventCategories
            .Include(ec => ec.Event)
            .FirstOrDefaultAsync(ec =>
                ec.EventID == eventId &&
                ec.CategoryID == categoryId);

        if (eventCategory == null)
            return NotFound(new
            {
                message = "Event category not found."
            });

        if (eventCategory.Event.OrganiserID != organiserId)
            return Forbid();

        var hasEnrolments = await _context.Enrolments
            .AnyAsync(en =>
                en.EventCategoryID == eventCategory.EventCategoryID);

        if (hasEnrolments)
            return Conflict(new
            {
                message = "Cannot remove a category with existing enrolments."
            });

        _context.EventCategories.Remove(eventCategory);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
