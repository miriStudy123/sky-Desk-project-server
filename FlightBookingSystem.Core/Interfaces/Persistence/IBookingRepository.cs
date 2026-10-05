using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.Entities;

namespace FlightBookingSystem.Core.Interfaces.Persistence;

public interface IBookingRepository : IRepository<Booking>
{
    /// <summary>One database-side page of the given user's bookings, newest first, with flight/seat details.</summary>
    Task<PagedResult<Booking>> GetForUserAsync(int userId, PaginationQuery query, CancellationToken cancellationToken = default);

    Task<Booking?> GetWithDetailsAsync(int bookingId, CancellationToken cancellationToken = default);

    Task<bool> ActiveBookingExistsForSeatAsync(int flightSeatId, CancellationToken cancellationToken = default);
}
