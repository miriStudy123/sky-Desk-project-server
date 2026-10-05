using FlightBookingSystem.Core.DTOs.Common;

namespace FlightBookingSystem.Core.DTOs.Flights;

/// <summary>
/// Optional filters for <c>GET /api/flights</c>, on top of the inherited paging parameters.
/// </summary>
public class FlightSearchQuery : PaginationQuery
{
    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public DateTime? DepartureFrom { get; set; }

    public DateTime? DepartureTo { get; set; }

    public string? Tag { get; set; }
}
