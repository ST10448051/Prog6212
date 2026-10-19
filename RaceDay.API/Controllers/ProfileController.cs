
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ProfileController(RaceDayDbContext context)
    {
        _context = context;
    }

    private int? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out var id)
            ? id
            : null;
    }

    /// <summary>
    /// Retrieves the currently authenticated user's profile.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserID == userId.Value)
            .Select(u => new
            {
                u.UserID,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new
            {
                message = "User profile not found."
            });

        return Ok(user);
    }

    /// <summary>
    /// Updates the currently authenticated user's profile.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        UpdateProfileDTO dto)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserID == userId.Value);

        if (user == null)
            return NotFound(new
            {
                message = "User profile not found."
            });

        var firstName = dto.FirstName.Trim();
        var lastName = dto.LastName.Trim();
        var email = dto.Email.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new
            {
                message = "First name, last name and email are required."
            });
        }

        var emailInUse = await _context.Users
            .AnyAsync(u =>
                u.Email == email &&
                u.UserID != userId.Value);

        if (emailInUse)
        {
            return Conflict(new
            {
                message = "This email address is already in use."
            });
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully.",
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.CreatedAt
        });
    }
}
