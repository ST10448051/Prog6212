
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all available race categories.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new
            {
                c.CategoryID,
                c.CategoryName,
                c.Description
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a specific category by its ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.CategoryID == id)
            .Select(c => new
            {
                c.CategoryID,
                c.CategoryName,
                c.Description
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        return Ok(category);
    }

    /// <summary>
    /// Creates a new race category.
    /// Organiser access required.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateCategory(
        CreateCategoryDTO dto)
    {
        var name = dto.CategoryName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "Category name cannot be empty."
            });
        }

        if (await _context.Categories
            .AnyAsync(c => c.CategoryName == name))
        {
            return Conflict(new
            {
                message = "This category already exists."
            });
        }

        var category = new Category
        {
            CategoryName = name,
            Description = dto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.CategoryID },
            new
            {
                category.CategoryID,
                category.CategoryName,
                category.Description
            });
    }

    /// <summary>
    /// Updates an existing race category.
    /// Organiser access required.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateCategory(
        int id, UpdateCategoryDTO dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        var name = dto.CategoryName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "Category name cannot be empty."
            });
        }

        if (await _context.Categories.AnyAsync(c =>
            c.CategoryName == name && c.CategoryID != id))
        {
            return Conflict(new
            {
                message = "Another category already uses this name."
            });
        }

        category.CategoryName = name;
        category.Description = dto.Description;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Category updated successfully."
        });
    }

    /// <summary>
    /// Deletes a category if it is not assigned to an event.
    /// Organiser access required.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        var isInUse = await _context.EventCategories
            .AnyAsync(ec => ec.CategoryID == id);

        if (isInUse)
        {
            return Conflict(new
            {
                message = "Cannot delete a category assigned to an event."
            });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
