
using System.Net;
using Xunit;

namespace RaceDay.API.Tests;

public class AuthenticationIntegrationTests
    : IClassFixture<RaceDayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(
        RaceDayWebApplicationFactory factory)
    {
        _client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing
                .WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });
    }

    [Fact]
    public async Task Profile_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/profile");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task MyEnrolments_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/enrolments/my");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task MyResults_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/results/my");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Events_WithoutLogin_AllowsPublicAccess()
    {
        var response = await _client.GetAsync("/api/events");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}
