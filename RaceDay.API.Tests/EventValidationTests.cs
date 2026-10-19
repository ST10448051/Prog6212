
using System.ComponentModel.DataAnnotations;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.API.Tests;

public class EventValidationTests
{
    private static bool IsValid(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        return Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);
    }

    [Fact]
    public void CreateCategory_ValidName_PassesValidation()
    {
        var dto = new CreateCategoryDTO
        {
            CategoryName = "Half Marathon",
            Description = "21.1 km running category"
        };

        Assert.True(IsValid(dto));
    }

    [Fact]
    public void CreateCategory_EmptyName_FailsValidation()
    {
        var dto = new CreateCategoryDTO
        {
            CategoryName = ""
        };

        Assert.False(IsValid(dto));
    }

    [Fact]
    public void CreateEnrolment_ValidCategoryId_PassesValidation()
    {
        var dto = new CreateEnrolmentDTO
        {
            EventCategoryID = 1
        };

        Assert.True(IsValid(dto));
    }

    [Fact]
    public void CreateEnrolment_ZeroCategoryId_FailsValidation()
    {
        var dto = new CreateEnrolmentDTO
        {
            EventCategoryID = 0
        };

        Assert.False(IsValid(dto));
    }

    [Fact]
    public void CreateWeatherReport_InvalidRainProbability_FailsValidation()
    {
        var dto = new CreateWeatherReportDTO
        {
            ReportDate = DateTime.UtcNow,
            RainProbabilityPercent = 150
        };

        Assert.False(IsValid(dto));
    }

    [Fact]
    public void CreateWeatherReport_ValidData_PassesValidation()
    {
        var dto = new CreateWeatherReportDTO
        {
            ReportDate = DateTime.UtcNow,
            TemperatureC = 24,
            Conditions = "Sunny",
            WindSpeedKmh = 12,
            RainProbabilityPercent = 10
        };

        Assert.True(IsValid(dto));
    }

    [Fact]
    public void CreateRoute_NegativeDistance_FailsValidation()
    {
        var dto = new CreateRouteDTO
        {
            RouteName = "Test Route",
            DistanceKm = -5
        };

        Assert.False(IsValid(dto));
    }

    [Fact]
    public void CreateRoute_ValidDistance_PassesValidation()
    {
        var dto = new CreateRouteDTO
        {
            RouteName = "Test Route",
            DistanceKm = 10,
            ElevationGainM = 150
        };

        Assert.True(IsValid(dto));
    }
}
