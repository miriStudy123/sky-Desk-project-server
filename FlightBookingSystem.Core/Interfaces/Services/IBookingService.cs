using FlightBookingSystem.Core.DTOs.Bookings;
using FlightBookingSystem.Core.DTOs.Common;

namespace FlightBookingSystem.Core.Interfaces.Services;

public interface IBookingService
{
    /// <summary>
    /// Books a seat for the given user. All state-dependent checks (seat exists, belongs to the flight,
    /// is available, not already taken) plus the status change and save happen here as one guarded unit.
    /// Throws <c>ConflictException</c> when a concurrent booking wins the race for the same seat.
    /// </summary>
    Task<BookingResponse> CreateAsync(int userId, CreateBookingRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<BookingResponse>> GetMyBookingsAsync(int userId, PaginationQuery query, CancellationToken cancellationToken = default);

    Task<BookingResponse> GetByIdAsync(int userId, bool isAdmin, int bookingId, CancellationToken cancellationToken = default);

    Task CancelAsync(int userId, bool isAdmin, int bookingId, CancellationToken cancellationToken = default);
}
