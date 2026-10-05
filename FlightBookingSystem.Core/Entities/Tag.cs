namespace FlightBookingSystem.Core.Entities;

/// <summary>
/// A free-form label that can be attached to flights (for example "Direct", "RedEye", "Domestic").
/// Related to <see cref="Flight"/> as many-to-many through <see cref="FlightTag"/>.
/// </summary>
public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    // Navigation - Tag N:M Flight (through FlightTag)
    public ICollection<FlightTag> FlightTags { get; set; } = new List<FlightTag>();
}
