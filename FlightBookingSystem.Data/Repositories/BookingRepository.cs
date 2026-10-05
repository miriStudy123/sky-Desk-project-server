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

    public async Task<PagedResult<Booking>> GetForUserAsync(
        int userId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var baseQuery = Set.AsNoTracking().Where(b => b.UserId == userId);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .Include(b => b.FlightSeat).ThenInclude(fs => fs.Seat)
            .Include(b => b.FlightSeat).ThenInclude(fs => fs.Flight)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.Id)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<Booking>(items, totalCount, query.Page, query.PageSize);
    }

    public Task<Booking?> GetWithDetailsAsync(int bookingId, CancellationToken cancellationToken = default)
        => Set.Include(b => b.FlightSeat).ThenInclude(fs => fs.Seat)
              .Include(b => b.FlightSeat).ThenInclude(fs => fs.Flight)
              .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

    public Task<bool> ActiveBookingExistsForSeatAsync(int flightSeatId, CancellationToken cancellationToken = default)
        => Set.AsNoTracking()
              .AnyAsync(b => b.FlightSeatId == flightSeatId && b.Status == BookingStatus.Confirmed, cancellationToken);
}
