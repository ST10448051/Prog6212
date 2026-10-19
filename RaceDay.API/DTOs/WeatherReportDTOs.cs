
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateWeatherReportDTO
{
    [Required]
    public DateTime? ReportDate { get; set; }

    [Range(typeof(decimal), "-100", "70")]
    public decimal? TemperatureC { get; set; }

    [StringLength(200)]
    public string? Conditions { get; set; }

    [Range(typeof(decimal), "0", "500")]
    public decimal? WindSpeedKmh { get; set; }

    [Range(0, 100)]
    public int? RainProbabilityPercent { get; set; }
}

public class UpdateWeatherReportDTO : CreateWeatherReportDTO
{
}
