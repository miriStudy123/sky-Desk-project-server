using System.ComponentModel.DataAnnotations;

namespace FlightBookingSystem.Core.DTOs.Flights;

/// <summary>Same fields as <see cref="UpdateFlightRequest"/>, plus the aircraft (fixed once the flight exists).</summary>
public class CreateFlightRequest : UpdateFlightRequest
{
    [Range(1, int.MaxValue)]
    public int AircraftId { get; set; }
}
