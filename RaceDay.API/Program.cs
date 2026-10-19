
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers
builder.Services.AddControllers();

// Configure Entity Framework Core and SQL Server
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RaceDayConnection")
    )
);

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
