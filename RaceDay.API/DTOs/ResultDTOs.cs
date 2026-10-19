
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateResultDTO
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EnrolmentID { get; set; }

    public TimeSpan? FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int? Position { get; set; }

    [Required]
    [RegularExpression(
        "^(Finished|DidNotFinish|Disqualified)$",
        ErrorMessage = "Invalid result status.")]
    public string ResultStatus { get; set; } = "Finished";
}

public class UpdateResultDTO
{
    public TimeSpan? FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int? Position { get; set; }

    [Required]
    [RegularExpression(
        "^(Finished|DidNotFinish|Disqualified)$",
        ErrorMessage = "Invalid result status.")]
    public string ResultStatus { get; set; } = "Finished";
}
