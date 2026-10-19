
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateEventCategoryDTO
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CategoryID { get; set; }

    [Range(0, 99999999.99)]
    public decimal EntryFee { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxParticipants { get; set; }
}

public class UpdateEventCategoryDTO
{
    [Range(0, 99999999.99)]
    public decimal EntryFee { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxParticipants { get; set; }
}
 