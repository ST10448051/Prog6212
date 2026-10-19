
namespace RaceDay.API.Models;

public class User
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Participant";
    public DateTime CreatedAt { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public class Event
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public string Status { get; set; } = "Upcoming";
    public DateTime CreatedAt { get; set; }
    public string EventType { get; set; } = "Run";
    public User Organiser { get; set; } = null!;
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    public Route? Route { get; set; }
    public ICollection<WeatherReport> WeatherReports { get; set; } = new List<WeatherReport>();
}

public class Category
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }

    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}

public class EventCategory
{
    public int EventCategoryID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public decimal EntryFee { get; set; }
    public int? MaxParticipants { get; set; }

    public Event Event { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public class Enrolment
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public int EventCategoryID { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "Confirmed";

    public User Participant { get; set; } = null!;
    public EventCategory EventCategory { get; set; } = null!;
    public Result? Result { get; set; }
}

public class Result
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string ResultStatus { get; set; } = "Finished";
    public DateTime RecordedAt { get; set; }

    public Enrolment Enrolment { get; set; } = null!;
}

public class Route
{
    public int RouteID { get; set; }
    public int EventID { get; set; }
    public string RouteName { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public int? ElevationGainM { get; set; }
    public string? RouteDescription { get; set; }
    public string? MapURL { get; set; }

    public Event Event { get; set; } = null!;
}

public class WeatherReport
{
    public int WeatherReportID { get; set; }
    public int EventID { get; set; }
    public DateTime ReportDate { get; set; }
    public decimal? TemperatureC { get; set; }
    public string? Conditions { get; set; }
    public decimal? WindSpeedKmh { get; set; }
    public int? RainProbabilityPercent { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Event Event { get; set; } = null!;
}
