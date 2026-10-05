namespace FlightBookingSystem.Core.Enums;

/// <summary>
/// Status of a <see cref="Entities.Booking"/>.
/// </summary>
public enum BookingStatus
{
    /// <summary>The booking is active and the seat is reserved for the user.</summary>
    Confirmed = 0,

    /// <summary>The booking was cancelled and the seat was released back to the pool.</summary>
    Cancelled = 1
}
