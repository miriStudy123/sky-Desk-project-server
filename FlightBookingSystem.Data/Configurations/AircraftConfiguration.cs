using FlightBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBookingSystem.Data.Configurations;

public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.ToTable("Aircraft");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Model).IsRequired().HasMaxLength(100);
        builder.Property(a => a.TotalSeats).IsRequired();

        // Aircraft 1:N Seat
        builder.HasMany(a => a.Seats)
            .WithOne(s => s.Aircraft)
            .HasForeignKey(s => s.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);

        // Aircraft 1:N Flight
        builder.HasMany(a => a.Flights)
            .WithOne(f => f.Aircraft)
            .HasForeignKey(f => f.AircraftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
