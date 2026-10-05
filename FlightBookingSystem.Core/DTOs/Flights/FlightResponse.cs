namespace FlightBookingSystem.Core.DTOs.Flights;

public class FlightResponse
{
    public int Id { get; set; }

    public string FlightNumber { get; set; } = null!;

    public string Origin { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public int AircraftId { get; set; }

    public string AircraftModel { get; set; } = null!;

    public int AvailableSeats { get; set; }

    public List<string> Tags { get; set; } = new();
}
