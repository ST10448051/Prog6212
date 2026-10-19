
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(
        DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<WeatherReport> WeatherReports => Set<WeatherReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users", t =>
                t.HasCheckConstraint("CK_Users_Role",
                    "[Role] IN ('Organiser', 'Participant')"));

            e.HasKey(x => x.UserID).HasName("PK_Users");
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20)
                .HasDefaultValue("Participant");
            e.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.Email)
                .IsUnique().HasDatabaseName("UQ_Users_Email");
        });

        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("Events", t =>
            {
                t.HasCheckConstraint("CK_Events_Distance", "[DistanceKm] > 0");
                t.HasCheckConstraint("CK_Events_EntryFee", "[EntryFee] >= 0");
                t.HasCheckConstraint("CK_Events_Status",
                    "[Status] IN ('Draft','Upcoming','Open','Closed','Completed','Cancelled')");
            });

            e.HasKey(x => x.EventID).HasName("PK_Events");
            e.Property(x => x.EventName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.EventDate).HasColumnType("date");
            e.Property(x => x.Location).HasMaxLength(200).IsRequired();
            e.Property(x => x.DistanceKm).HasColumnType("decimal(6,2)");
            e.Property(x => x.EntryFee).HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m);
            e.Property(x => x.Status).HasMaxLength(30)
                .HasDefaultValue("Upcoming");
            e.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(x => x.Organiser)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.OrganiserID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Events_Organiser");
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.CategoryID).HasName("PK_Categories");
            e.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => x.CategoryName).IsUnique()
                .HasDatabaseName("UQ_Categories_CategoryName");
        });

        modelBuilder.Entity<EventCategory>(e =>
        {
            e.ToTable("EventCategories", t =>
            {
                t.HasCheckConstraint("CK_EventCategories_EntryFee",
                    "[EntryFee] >= 0");
                t.HasCheckConstraint("CK_EventCategories_MaxParticipants",
                    "[MaxParticipants] IS NULL OR [MaxParticipants] > 0");
            });

            e.HasKey(x => x.EventCategoryID)
                .HasName("PK_EventCategories");
            e.Property(x => x.EntryFee).HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m);

            e.HasIndex(x => new { x.EventID, x.CategoryID })
                .IsUnique()
                .HasDatabaseName("UQ_EventCategories_Event_Category");

            e.HasOne(x => x.Event)
                .WithMany(x => x.EventCategories)
                .HasForeignKey(x => x.EventID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EventCategories_Event");

            e.HasOne(x => x.Category)
                .WithMany(x => x.EventCategories)
                .HasForeignKey(x => x.CategoryID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EventCategories_Category");
        });

        modelBuilder.Entity<Enrolment>(e =>
        {
            e.ToTable("Enrolments", t =>
                t.HasCheckConstraint("CK_Enrolments_Status",
                    "[Status] IN ('Pending','Confirmed','Cancelled','Completed')"));

            e.HasKey(x => x.EnrolmentID).HasName("PK_Enrolments");
            e.Property(x => x.EnrolmentDate)
                .HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(x => x.Status).HasMaxLength(30)
                .HasDefaultValue("Confirmed");

            e.HasIndex(x => new { x.ParticipantID, x.EventCategoryID })
                .IsUnique()
                .HasDatabaseName("UQ_Enrolments_Participant_EventCategory");

            e.HasOne(x => x.Participant)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Enrolments_Participant");

            e.HasOne(x => x.EventCategory)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.EventCategoryID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Enrolments_EventCategory");
        });

        modelBuilder.Entity<Result>(e =>
        {
            e.ToTable("Results", t =>
            {
                t.HasCheckConstraint("CK_Results_Position",
                    "[Position] IS NULL OR [Position] > 0");
                t.HasCheckConstraint("CK_Results_Status",
                    "[ResultStatus] IN ('Finished','DidNotFinish','Disqualified')");
            });

            e.HasKey(x => x.ResultID).HasName("PK_Results");
            e.Property(x => x.FinishTime).HasColumnType("time(0)");
            e.Property(x => x.ResultStatus).HasMaxLength(30)
                .HasDefaultValue("Finished");
            e.Property(x => x.RecordedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasIndex(x => x.EnrolmentID).IsUnique()
                .HasDatabaseName("UQ_Results_Enrolment");

            e.HasOne(x => x.Enrolment)
                .WithOne(x => x.Result)
                .HasForeignKey<Result>(x => x.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Results_Enrolment");
        });

        modelBuilder.Entity<Route>(e =>
        {
            e.ToTable("Routes", t =>
            {
                t.HasCheckConstraint("CK_Routes_Distance", "[DistanceKm] > 0");
                t.HasCheckConstraint("CK_Routes_Elevation",
                    "[ElevationGainM] IS NULL OR [ElevationGainM] >= 0");
            });

            e.HasKey(x => x.RouteID).HasName("PK_Routes");
            e.Property(x => x.RouteName).HasMaxLength(200).IsRequired();
            e.Property(x => x.DistanceKm).HasColumnType("decimal(6,2)");
            e.Property(x => x.RouteDescription).HasMaxLength(1500);
            e.Property(x => x.MapURL).HasMaxLength(500);

            e.HasIndex(x => x.EventID).IsUnique()
                .HasDatabaseName("UQ_Routes_Event");

            e.HasOne(x => x.Event)
                .WithOne(x => x.Route)
                .HasForeignKey<Route>(x => x.EventID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Routes_Event");
        });

        modelBuilder.Entity<WeatherReport>(e =>
        {
            e.ToTable("WeatherReports", t =>
            {
                t.HasCheckConstraint("CK_WeatherReports_Wind",
                    "[WindSpeedKmh] IS NULL OR [WindSpeedKmh] >= 0");
                t.HasCheckConstraint("CK_WeatherReports_RainProbability",
                    "[RainProbabilityPercent] IS NULL OR [RainProbabilityPercent] BETWEEN 0 AND 100");
            });

            e.HasKey(x => x.WeatherReportID)
                .HasName("PK_WeatherReports");
            e.Property(x => x.ReportDate).HasColumnType("date");
            e.Property(x => x.TemperatureC).HasColumnType("decimal(5,2)");
            e.Property(x => x.Conditions).HasMaxLength(100);
            e.Property(x => x.WindSpeedKmh).HasColumnType("decimal(5,2)");
            e.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasIndex(x => new { x.EventID, x.ReportDate })
                .IsUnique()
                .HasDatabaseName("UQ_WeatherReports_Event_Date");

            e.HasOne(x => x.Event)
                .WithMany(x => x.WeatherReports)
                .HasForeignKey(x => x.EventID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_WeatherReports_Event");
        });
    }
}
