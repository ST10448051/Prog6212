
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateCategoryDTO
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateCategoryDTO : CreateCategoryDTO
{
}
