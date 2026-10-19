
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CreateRouteDTO
{
    [Required]
    [StringLength(150)]
    public string RouteName { get; set; } = "";

    [Range(typeof(decimal), "0.01", "10000")]
    public decimal DistanceKm { get; set; }

    [Range(0, 100000)]
    public int? ElevationGainM { get; set; }

    [StringLength(2000)]
    public string? RouteDescription { get; set; }

    [Url]
    [StringLength(1000)]
    public string? MapURL { get; set; }
}

public class UpdateRouteDTO : CreateRouteDTO
{
}
