namespace FlightBookingSystem.Core.Entities;

/// <summary>
/// A physical seat position on an <see cref="Aircraft"/> (for example row 12, letter C).
/// The bookable per-flight instance is <see cref="FlightSeat"/>.
/// </summary>
public class Seat
{
    public int Id { get; set; }

    public int AircraftId { get; set; }
    public Aircraft Aircraft { get; set; } = null!;

    public int RowNumber { get; set; }

    public string SeatLetter { get; set; } = null!;

    // Navigation - Seat 1:N FlightSeat
    public ICollection<FlightSeat> FlightSeats { get; set; } = new List<FlightSeat>();
}
