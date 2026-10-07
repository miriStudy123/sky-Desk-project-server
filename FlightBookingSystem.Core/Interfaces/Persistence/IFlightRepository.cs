using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.DTOs.Flights;
using FlightBookingSystem.Core.Entities;

namespace FlightBookingSystem.Core.Interfaces.Persistence;

public interface IFlightRepository : IRepository<Flight>
{
    /// <summary>
    /// Returns one database-side page of flights matching <paramref name="query"/>.
    /// Paging is applied with <c>Skip</c>/<c>Take</c> before the query is materialized, and related
    /// data (aircraft, tags, seat availability) is eager-loaded to avoid the N+1 pattern.
    /// </summary>
    Task<PagedResult<Flight>> SearchAsync(FlightSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>Loads a flight with its aircraft and tags.</summary>
    Task<Flight?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Loads a flight as a tracked entity together with its <c>FlightTags</c>, for updates.</summary>
    Task<Flight?> GetTrackedWithTagsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> HasConfirmedBookingsAsync(int flightId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the flight's cancelled bookings for deletion (committed by the next save). They would otherwise
    /// block deleting the flight, since its seats cascade-delete but a booking restricts deleting its seat.
    /// </summary>
    Task RemoveCancelledBookingsAsync(int flightId, CancellationToken cancellationToken = default);

    Task<bool> FlightNumberExistsAsync(string flightNumber, int? excludingFlightId, CancellationToken cancellationToken = default);

    Task<Dictionary<string, Tag>> GetOrCreateTagsAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
}
