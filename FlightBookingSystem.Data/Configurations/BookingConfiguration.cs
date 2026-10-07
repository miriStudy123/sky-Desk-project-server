using FlightBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBookingSystem.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Reference).IsRequired().HasMaxLength(12);
        builder.HasIndex(b => b.Reference).IsUnique();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // EF creates an index for each foreign key below (UserId, FlightSeatId) by convention.

        // Many-to-one: Booking -> User
        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: Booking -> FlightSeat
        builder.HasOne(b => b.FlightSeat)
            .WithMany(fs => fs.Bookings)
            .HasForeignKey(b => b.FlightSeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
