using FlightBookingSystem.Core.Enums;

namespace FlightBookingSystem.Core.Entities;

/// <summary>
/// A specific seat on a specific flight - the scarce resource of the system.
/// This is the entity protected by optimistic concurrency: two users may read the same
/// <see cref="FlightSeat"/> while it is <see cref="SeatStatus.Available"/> and try to book it
/// concurrently, but only one write may win. The losing write triggers a
/// <c>DbUpdateConcurrencyException</c> which the service layer translates into HTTP 409.
/// The concurrency token itself (PostgreSQL's <c>xmin</c> system column) is configured as a
/// shadow property in <c>FlightSeatConfiguration</c>, so it has no CLR property here.
/// </summary>
public class FlightSeat
{
    public int Id { get; set; }

    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;

    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;

    public SeatStatus Status { get; set; } = SeatStatus.Available;

    // Navigation - FlightSeat 1:N Booking
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
