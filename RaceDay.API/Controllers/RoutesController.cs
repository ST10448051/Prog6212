
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class RoutesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public RoutesController(RaceDayDbContext context)
    {
        _context = context;
    }

    private int? CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var id)
            ? id
            : null;
    }

    /// <summary>
    /// Retrieves all routes for a particular event.
    /// </summary>
    [HttpGet("events/{eventId:int}/routes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventRoutes(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == eventId);

        if (!eventExists)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        var routes = await _context.Routes
            .AsNoTracking()
            .Where(r => r.EventID == eventId)
            .OrderBy(r => r.RouteName)
            .Select(r => new
            {
                r.RouteID,
                r.EventID,
                r.RouteName,
                r.DistanceKm,
                r.ElevationGainM,
                r.RouteDescription,
                r.MapURL
            })
            .ToListAsync();

        return Ok(routes);
    }

    /// <summary>
    /// Retrieves one route by its ID.
    /// </summary>
    [HttpGet("routes/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoute(int id)
    {
        var route = await _context.Routes
            .AsNoTracking()
            .Where(r => r.RouteID == id)
            .Select(r => new
            {
                r.RouteID,
                r.EventID,
                r.RouteName,
                r.DistanceKm,
                r.ElevationGainM,
                r.RouteDescription,
                r.MapURL
            })
            .FirstOrDefaultAsync();

        if (route == null)
        {
            return NotFound(new
            {
                message = "Route not found."
            });
        }

        return Ok(route);
    }

    /// <summary>
    /// Adds a route to an event owned by the Organiser.
    /// </summary>
    [HttpPost("events/{eventId:int}/routes")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateRoute(
        int eventId,
        CreateRouteDTO dto)
    {
        var organiserId = CurrentUserId();

        if (organiserId == null)
            return Unauthorized();

        var raceEvent = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventID == eventId);

        if (raceEvent == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        if (raceEvent.OrganiserID != organiserId.Value)
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.RouteName))
        {
            return BadRequest(new
            {
                message = "Route name is required."
            });
        }

        var route = new RaceDay.API.Models.Route
        {
            EventID = eventId,
            RouteName = dto.RouteName.Trim(),
            DistanceKm = dto.DistanceKm,
            ElevationGainM = dto.ElevationGainM,
            RouteDescription = dto.RouteDescription,
            MapURL = dto.MapURL
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetRoute),
            new { id = route.RouteID },
            new
            {
                route.RouteID,
                route.EventID,
                route.RouteName,
                route.DistanceKm,
                route.ElevationGainM,
                route.RouteDescription,
                route.MapURL
            });
    }

    /// <summary>
    /// Updates a route owned by the event's Organiser.
    /// </summary>
    [HttpPut("routes/{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateRoute(
        int id,
        UpdateRouteDTO dto)
    {
        var organiserId = CurrentUserId();

        if (organiserId == null)
            return Unauthorized();

        var route = await _context.Routes
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.RouteID == id);

        if (route == null)
        {
            return NotFound(new
            {
                message = "Route not found."
            });
        }

        if (route.Event.OrganiserID != organiserId.Value)
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.RouteName))
        {
            return BadRequest(new
            {
                message = "Route name is required."
            });
        }

        route.RouteName = dto.RouteName.Trim();
        route.DistanceKm = dto.DistanceKm;
        route.ElevationGainM = dto.ElevationGainM;
        route.RouteDescription = dto.RouteDescription;
        route.MapURL = dto.MapURL;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Route updated successfully."
        });
    }

    /// <summary>
    /// Deletes a route owned by the event's Organiser.
    /// </summary>
    [HttpDelete("routes/{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        var organiserId = CurrentUserId();

        if (organiserId == null)
            return Unauthorized();

        var route = await _context.Routes
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.RouteID == id);

        if (route == null)
        {
            return NotFound(new
            {
                message = "Route not found."
            });
        }

        if (route.Event.OrganiserID != organiserId.Value)
            return Forbid();

        _context.Routes.Remove(route);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
