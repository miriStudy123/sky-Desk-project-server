namespace FlightBookingSystem.Core.Entities;

/// <summary>
/// A scheduled flight operated by a specific <see cref="Aircraft"/>.
/// </summary>
public class Flight
{
    public int Id { get; set; }

    public string FlightNumber { get; set; } = null!;

    public string Origin { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public int AircraftId { get; set; }
    public Aircraft Aircraft { get; set; } = null!;

    public ICollection<FlightSeat> FlightSeats { get; set; } = new List<FlightSeat>();

    
    public ICollection<FlightTag> FlightTags { get; set; } = new List<FlightTag>();
}
