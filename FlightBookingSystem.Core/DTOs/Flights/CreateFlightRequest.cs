using System.ComponentModel.DataAnnotations;

namespace FlightBookingSystem.Core.DTOs.Flights;

public class CreateFlightRequest
{
    [Required]
    [StringLength(10)]
    public string FlightNumber { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Origin { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Destination { get; set; } = null!;

    [Required]
    public DateTime DepartureTime { get; set; }

    [Required]
    public DateTime ArrivalTime { get; set; }

    [Range(1, int.MaxValue)]
    public int AircraftId { get; set; }

    /// <summary>Optional tag names to associate with the flight (created on demand).</summary>
    public List<string> Tags { get; set; } = new();
}
