using FlightBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBookingSystem.Data.Configurations;

public class FlightSeatConfiguration : IEntityTypeConfiguration<FlightSeat>
{
    public void Configure(EntityTypeBuilder<FlightSeat> builder)
    {
        builder.ToTable("FlightSeats");
        builder.HasKey(fs => fs.Id);

        builder.Property(fs => fs.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Optimistic concurrency token - PostgreSQL's xmin system column, exposed as a shadow
        // property (Npgsql maps a uint property named "xmin" to it automatically).
        // Every UPDATE/DELETE carries the loaded value in its WHERE clause; a stale value
        // affects zero rows and EF raises DbUpdateConcurrencyException.
        builder.Property<uint>("xmin")
            .IsRowVersion();

        builder.HasIndex(fs => new { fs.FlightId, fs.SeatId }).IsUnique();
        builder.HasIndex(fs => fs.Status);

        // Many-to-one: FlightSeat -> Seat
        builder.HasOne(fs => fs.Seat)
            .WithMany(s => s.FlightSeats)
            .HasForeignKey(fs => fs.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        // FlightSeat 1:N Booking is configured on the dependent side, in BookingConfiguration.
    }
}
