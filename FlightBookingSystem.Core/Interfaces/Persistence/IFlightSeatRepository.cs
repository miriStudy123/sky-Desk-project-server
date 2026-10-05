using FlightBookingSystem.Core.Entities;

namespace FlightBookingSystem.Core.Interfaces.Persistence;

public interface IFlightSeatRepository : IRepository<FlightSeat>
{
    /// <summary>All seats for a flight, including row/letter, for display. Read-only (no tracking).</summary>
    Task<IReadOnlyList<FlightSeat>> GetByFlightAsync(int flightId, bool onlyAvailable, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a single <see cref="FlightSeat"/> as a tracked entity (including its <c>RowVersion</c>)
    /// so the booking flow can update it under optimistic concurrency.
    /// </summary>
    Task<FlightSeat?> GetTrackedForBookingAsync(int flightSeatId, CancellationToken cancellationToken = default);

    Task<bool> FlightExistsAsync(int flightId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Available-seat count per flight, resolved in a single grouped query (no N+1) for the given flight ids.
    /// Flights with no available seats are still present in the result with a count of 0.
    /// </summary>
    Task<IReadOnlyDictionary<int, int>> GetAvailableSeatCountsAsync(
        IReadOnlyCollection<int> flightIds, CancellationToken cancellationToken = default);
}
