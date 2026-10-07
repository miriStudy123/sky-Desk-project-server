using FlightBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBookingSystem.Data.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.ToTable("Flights");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FlightNumber).IsRequired().HasMaxLength(10);
        builder.Property(f => f.Origin).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Destination).IsRequired().HasMaxLength(100);

        builder.HasIndex(f => f.FlightNumber);
        builder.HasIndex(f => new { f.Origin, f.Destination, f.DepartureTime });

        // Many-to-one: Flight -> Aircraft
        builder.HasOne(f => f.Aircraft)
            .WithMany(a => a.Flights)
            .HasForeignKey(f => f.AircraftId)
            .OnDelete(DeleteBehavior.Restrict);

        // Flight 1:N FlightSeat
        builder.HasMany(f => f.FlightSeats)
            .WithOne(fs => fs.Flight)
            .HasForeignKey(fs => fs.FlightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
