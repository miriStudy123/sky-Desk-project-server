namespace FlightBookingSystem.Core.DTOs.Seats;

public class SeatResponse
{
    /// <summary>The <see cref="Entities.FlightSeat"/> id - the value passed to <c>POST /api/bookings</c>.</summary>
    public int FlightSeatId { get; set; }

    public int RowNumber { get; set; }

    public string SeatLetter { get; set; } = null!;

    public string Status { get; set; } = null!;
}
