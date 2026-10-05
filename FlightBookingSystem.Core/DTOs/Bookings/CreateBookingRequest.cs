using System.ComponentModel.DataAnnotations;

namespace FlightBookingSystem.Core.DTOs.Bookings;

public class CreateBookingRequest
{
    /// <summary>The <see cref="Entities.FlightSeat"/> the caller wants to book.</summary>
    [Range(1, int.MaxValue)]
    public int FlightSeatId { get; set; }
}
