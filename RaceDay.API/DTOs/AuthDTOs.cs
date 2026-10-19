
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class RegisterDTO
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = "";

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "Participant";
}

public class LoginDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}
