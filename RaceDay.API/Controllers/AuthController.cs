
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(
        RaceDayDbContext context,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Registers a new Organiser or Participant.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        if (dto.Role != "Organiser" && dto.Role != "Participant")
        {
            return BadRequest(new
            {
                message = "Role must be Organiser or Participant."
            });
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            Role = dto.Role
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "Unable to register this account."
            });
        }

        return StatusCode(201, new
        {
            message = "Registration successful.",
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role
        });
    }

    /// <summary>
    /// Authenticates a user and creates a login session.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, dto.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(user, dto.Password);

            await _context.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Ok(new
        {
            message = "Login successful.",
            user.UserID,
            user.FirstName,
            user.Role
        });
    }

    /// <summary>
    /// Returns the currently authenticated user's information.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserID == userId)
            .Select(u => new
            {
                u.UserID,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }
}
