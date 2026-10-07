using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Enums;
using FlightBookingSystem.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingSystem.Data.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>Bookings with the seat and flight details a <c>BookingResponse</c> needs (tracked).</summary>
    private IQueryable<Booking> WithDetails()
        => Set.Include(b => b.FlightSeat).ThenInclude(fs => fs.Seat)
              .Include(b => b.FlightSeat).ThenInclude(fs => fs.Flight);

    public Task<PagedResult<Booking>> GetForUserAsync(
        int userId, PaginationQuery query, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(
            WithDetails().AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.Id),
            query, cancellationToken);

    public Task<Booking?> GetWithDetailsAsync(int bookingId, CancellationToken cancellationToken = default)
        => WithDetails().FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

    public Task<bool> ActiveBookingExistsForSeatAsync(int flightSeatId, CancellationToken cancellationToken = default)
        => Set.AsNoTracking()
              .AnyAsync(b => b.FlightSeatId == flightSeatId && b.Status == BookingStatus.Confirmed, cancellationToken);
}
