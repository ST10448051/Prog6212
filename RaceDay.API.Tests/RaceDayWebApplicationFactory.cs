
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.API.Data;

namespace RaceDay.API.Tests;

public class RaceDayWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the application's SQL Server DbContext.
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

            // Remove the original DbContext registration.
            services.RemoveAll<RaceDayDbContext>();

            // Create a temporary in-memory SQLite database.
            _connection = new SqliteConnection(
                "Data Source=:memory:");

            _connection.Open();

            services.AddDbContext<RaceDayDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Build a temporary service provider and create
            // the database schema before running tests.
            using var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<RaceDayDbContext>();

            context.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing: disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
