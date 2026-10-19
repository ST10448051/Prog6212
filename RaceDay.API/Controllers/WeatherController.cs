
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
public class WeatherController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public WeatherController(RaceDayDbContext context)
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
    /// Retrieves all weather reports for an event.
    /// </summary>
    [HttpGet("events/{eventId:int}/weather")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventWeather(int eventId)
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

        var reports = await _context.WeatherReports
            .AsNoTracking()
            .Where(w => w.EventID == eventId)
            .OrderByDescending(w => w.ReportDate)
            .Select(w => new
            {
                w.WeatherReportID,
                w.EventID,
                w.ReportDate,
                w.TemperatureC,
                w.Conditions,
                w.WindSpeedKmh,
                w.RainProbabilityPercent,
                w.UpdatedAt
            })
            .ToListAsync();

        return Ok(reports);
    }

    /// <summary>
    /// Retrieves a weather report by its ID.
    /// </summary>
    [HttpGet("weather/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWeatherReport(int id)
    {
        var report = await _context.WeatherReports
            .AsNoTracking()
            .Where(w => w.WeatherReportID == id)
            .Select(w => new
            {
                w.WeatherReportID,
                w.EventID,
                w.ReportDate,
                w.TemperatureC,
                w.Conditions,
                w.WindSpeedKmh,
                w.RainProbabilityPercent,
                w.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return NotFound(new
            {
                message = "Weather report not found."
            });
        }

        return Ok(report);
    }

    /// <summary>
    /// Creates a weather report for an event owned by the Organiser.
    /// </summary>
    [HttpPost("events/{eventId:int}/weather")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateWeatherReport(
        int eventId,
        CreateWeatherReportDTO dto)
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

        var report = new WeatherReport
        {
            EventID = eventId,
            ReportDate = dto.ReportDate!.Value,
            TemperatureC = dto.TemperatureC,
            Conditions = dto.Conditions?.Trim(),
            WindSpeedKmh = dto.WindSpeedKmh,
            RainProbabilityPercent = dto.RainProbabilityPercent,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WeatherReports.Add(report);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetWeatherReport),
            new { id = report.WeatherReportID },
            new
            {
                report.WeatherReportID,
                report.EventID,
                report.ReportDate,
                report.TemperatureC,
                report.Conditions,
                report.WindSpeedKmh,
                report.RainProbabilityPercent,
                report.UpdatedAt
            });
    }

    /// <summary>
    /// Updates a weather report owned by the event's Organiser.
    /// </summary>
    [HttpPut("weather/{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateWeatherReport(
        int id,
        UpdateWeatherReportDTO dto)
    {
        var organiserId = CurrentUserId();

        if (organiserId == null)
            return Unauthorized();

        var report = await _context.WeatherReports
            .Include(w => w.Event)
            .FirstOrDefaultAsync(w => w.WeatherReportID == id);

        if (report == null)
        {
            return NotFound(new
            {
                message = "Weather report not found."
            });
        }

        if (report.Event.OrganiserID != organiserId.Value)
            return Forbid();

        report.ReportDate = dto.ReportDate!.Value;
        report.TemperatureC = dto.TemperatureC;
        report.Conditions = dto.Conditions?.Trim();
        report.WindSpeedKmh = dto.WindSpeedKmh;
        report.RainProbabilityPercent = dto.RainProbabilityPercent;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Weather report updated successfully.",
            report.WeatherReportID,
            report.UpdatedAt
        });
    }

    /// <summary>
    /// Deletes a weather report owned by the event's Organiser.
    /// </summary>
    [HttpDelete("weather/{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteWeatherReport(int id)
    {
        var organiserId = CurrentUserId();

        if (organiserId == null)
            return Unauthorized();

        var report = await _context.WeatherReports
            .Include(w => w.Event)
            .FirstOrDefaultAsync(w => w.WeatherReportID == id);

        if (report == null)
        {
            return NotFound(new
            {
                message = "Weather report not found."
            });
        }

        if (report.Event.OrganiserID != organiserId.Value)
            return Forbid();

        _context.WeatherReports.Remove(report);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
