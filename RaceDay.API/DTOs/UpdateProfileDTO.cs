
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class UpdateProfileDTO
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = "";

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = "";
}
