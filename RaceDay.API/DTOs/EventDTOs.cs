using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateEventDTO
{
    [Required]
    [StringLength(200)]
    public string EventName { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime? EventDate { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = "";

    [Range(0.01, 9999.99)]
    public decimal DistanceKm { get; set; }

    [Range(0, 99999999.99)]
    public decimal EntryFee { get; set; }

    [Required]
    public string Status { get; set; } = "Upcoming";

    [Required]
    [RegularExpression("^(Run|Walk|Cycle)$",
    ErrorMessage = "Event type must be Run, Walk, or Cycle.")]
    public string EventType { get; set; } = "Run";

}

public class UpdateEventDTO : CreateEventDTO
{
}