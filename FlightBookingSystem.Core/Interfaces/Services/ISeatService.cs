using FlightBookingSystem.Core.DTOs.Seats;

namespace FlightBookingSystem.Core.Interfaces.Services;

public interface ISeatService
{
    Task<IReadOnlyList<SeatResponse>> GetSeatsForFlightAsync(int flightId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeatResponse>> GetAvailableSeatsForFlightAsync(int flightId, CancellationToken cancellationToken = default);
}
