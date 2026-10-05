using FlightBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingSystem.Data;

/// <summary>
/// EF Core Code-First context. Entity mapping is defined with the Fluent API in the
/// <c>Configurations</c> folder and applied via <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>.
/// </summary>
public class AppDbContext : DbContext
{
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
}
