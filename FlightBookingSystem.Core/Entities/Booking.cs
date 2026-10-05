using FlightBookingSystem.Core.Enums;

namespace FlightBookingSystem.Core.Entities;

/// <summary>
/// A user's reservation of one <see cref="FlightSeat"/>.
/// </summary>
public class Booking
{
    public int Id { get; set; }

    /// <summary>Short human-friendly confirmation code, unique across all bookings.</summary>
    public string Reference { get; set; } = null!;//קוד אישור הזמנה

    public int UserId { get; set; }//id שהזמין
    public User User { get; set; } = null!;

    public int FlightSeatId { get; set; }
    public FlightSeat FlightSeat { get; set; } = null!;

    public DateTime BookingDate { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
}
