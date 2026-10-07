using FlightBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingSystem.Data;

/// <summary>
/// EF Core Code-First context. Entity mapping is defined with the Fluent API in the
/// <c>Configurations</c> folder and applied via <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Npgsql 6+ refuses to write a <see cref="DateTimeKind.Utc"/> value (e.g. <c>DateTime.UtcNow</c>) to a
    /// <c>timestamp without time zone</c> column. Legacy timestamp behavior accepts any Kind, matching how
    /// SQL Server's <c>datetime2</c> behaved. Set in the static constructor so it applies before Npgsql is
    /// first used - at runtime, in tests and in the EF design-time tooling alike.
    /// </summary>
    static AppDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Aircraft> Aircraft => Set<Aircraft>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<FlightSeat> FlightSeats => Set<FlightSeat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<FlightTag> FlightTags => Set<FlightTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Maps every <see cref="DateTime"/> to PostgreSQL's <c>timestamp without time zone</c> instead of
    /// Npgsql's default <c>timestamp with time zone</c>. The latter requires every value to carry
    /// <see cref="DateTimeKind.Utc"/> and throws otherwise; this app stores naive timestamps (as SQL
    /// Server's <c>datetime2</c> did) and doesn't want that restriction on values coming from API requests.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
    }
}
