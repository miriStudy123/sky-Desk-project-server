namespace FlightBookingSystem.Core.Enums;

/// <summary>
/// Lifecycle status of a specific seat on a specific flight (<see cref="Entities.FlightSeat"/>).
/// </summary>
public enum SeatStatus
{
    /// <summary>The seat can be booked.</summary>
    Available = 0,

    /// <summary>The seat is temporarily held while a booking is being completed.</summary>
    Held = 1,

    /// <summary>The seat is booked and is no longer offered to other users.</summary>
    Booked = 2
}
