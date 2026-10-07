using FlightBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBookingSystem.Data.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SeatLetter).IsRequired().HasMaxLength(2);

        // Many-to-one: Seat -> Aircraft
        builder.HasOne(s => s.Aircraft)
            .WithMany(a => a.Seats)
            .HasForeignKey(s => s.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.AircraftId, s.RowNumber, s.SeatLetter }).IsUnique();
    }
}
