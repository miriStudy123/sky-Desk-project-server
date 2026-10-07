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

        // Relationships are configured on the dependent side: Seat -> Aircraft in SeatConfiguration,
        // Flight -> Aircraft in FlightConfiguration.
    }
}
