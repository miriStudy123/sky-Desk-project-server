using FlightBookingSystem.Core.DTOs.Aircraft;

namespace FlightBookingSystem.Core.Interfaces.Services;

public interface IAircraftService
{
    Task<IReadOnlyList<AircraftResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
