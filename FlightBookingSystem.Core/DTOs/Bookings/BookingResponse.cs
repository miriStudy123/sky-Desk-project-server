namespace FlightBookingSystem.Core.DTOs.Bookings;

public class BookingResponse
{
    public int Id { get; set; }

    public string Reference { get; set; } = null!;

    public int FlightSeatId { get; set; }

    public int FlightId { get; set; }

    public string FlightNumber { get; set; } = null!;

    public string Origin { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public DateTime DepartureTime { get; set; }

    public int RowNumber { get; set; }

    public string SeatLetter { get; set; } = null!;

    public DateTime BookingDate { get; set; }

    public string Status { get; set; } = null!;
}
