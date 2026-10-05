namespace FlightBookingSystem.Core.Entities;

/// <summary>
/// Join entity implementing the many-to-many relationship between <see cref="Flight"/> and <see cref="Tag"/>.
/// Modelled explicitly (rather than as a skip navigation) so extra columns can be added later if needed.
/// </summary>
public class FlightTag
{
    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
