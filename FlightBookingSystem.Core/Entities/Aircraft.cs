namespace FlightBookingSystem.Core.Entities;

/// <summary>
/// A physical aircraft type with a fixed seat map. Reused across many flights.
/// </summary>
public class Aircraft
{
    public int Id { get; set; }

    public string Model { get; set; } = null!;

    public int TotalSeats { get; set; }

    // Navigation - Aircraft 1:N Seat
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();

    // Navigation - Aircraft 1:N Flight
    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
