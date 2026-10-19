
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateEnrolmentDTO
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EventCategoryID { get; set; }
}
