namespace FlightBookingSystem.Core.DTOs.Aircraft;

public class AircraftResponse
{
    public int Id { get; set; }

    public string Model { get; set; } = null!;

    public int TotalSeats { get; set; }
}
