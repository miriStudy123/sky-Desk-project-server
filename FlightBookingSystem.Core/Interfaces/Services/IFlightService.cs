using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.DTOs.Flights;

namespace FlightBookingSystem.Core.Interfaces.Services;

public interface IFlightService
{
    Task<PagedResult<FlightResponse>> SearchAsync(FlightSearchQuery query, CancellationToken cancellationToken = default);

    Task<FlightResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<FlightResponse> CreateAsync(CreateFlightRequest request, CancellationToken cancellationToken = default);

    Task<FlightResponse> UpdateAsync(int id, UpdateFlightRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
