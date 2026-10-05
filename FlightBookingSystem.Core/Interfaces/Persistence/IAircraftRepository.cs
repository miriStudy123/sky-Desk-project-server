using FlightBookingSystem.Core.Entities;

namespace FlightBookingSystem.Core.Interfaces.Persistence;

public interface IAircraftRepository : IRepository<Aircraft>
{
    /// <summary>Loads an aircraft together with its seat map.</summary>
    Task<Aircraft?> GetWithSeatsAsync(int aircraftId, CancellationToken cancellationToken = default);
}
