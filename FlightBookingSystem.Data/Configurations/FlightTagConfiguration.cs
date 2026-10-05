using FlightBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBookingSystem.Data.Configurations;

/// <summary>
/// Join table for the many-to-many relationship Flight N:M Tag.
/// </summary>
public class FlightTagConfiguration : IEntityTypeConfiguration<FlightTag>
{
    public void Configure(EntityTypeBuilder<FlightTag> builder)
    {
        builder.ToTable("FlightTags");
        builder.HasKey(ft => new { ft.FlightId, ft.TagId });

        builder.HasOne(ft => ft.Flight)
            .WithMany(f => f.FlightTags)
            .HasForeignKey(ft => ft.FlightId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ft => ft.Tag)
            .WithMany(t => t.FlightTags)
            .HasForeignKey(ft => ft.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
