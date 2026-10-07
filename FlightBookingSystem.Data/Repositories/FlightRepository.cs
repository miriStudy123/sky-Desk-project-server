using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.DTOs.Flights;
using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Enums;
using FlightBookingSystem.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingSystem.Data.Repositories;

public class FlightRepository : Repository<Flight>, IFlightRepository
{
    public FlightRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>Read-only flights with their aircraft and tags - everything a <c>FlightResponse</c> needs.</summary>
    private IQueryable<Flight> WithDetails()
        => Set.AsNoTracking()
              .Include(f => f.Aircraft)
              .Include(f => f.FlightTags).ThenInclude(ft => ft.Tag);

    public Task<PagedResult<Flight>> SearchAsync(FlightSearchQuery query, CancellationToken cancellationToken = default)
    {
        var q = WithDetails();

        if (!string.IsNullOrWhiteSpace(query.Origin))
            q = q.Where(f => f.Origin == query.Origin);

        if (!string.IsNullOrWhiteSpace(query.Destination))
            q = q.Where(f => f.Destination == query.Destination);

        if (query.DepartureFrom.HasValue)
            q = q.Where(f => f.DepartureTime >= query.DepartureFrom.Value);

        if (query.DepartureTo.HasValue)
            q = q.Where(f => f.DepartureTime <= query.DepartureTo.Value);

        if (!string.IsNullOrWhiteSpace(query.Tag))
            q = q.Where(f => f.FlightTags.Any(ft => ft.Tag.Name == query.Tag));

        return ToPagedResultAsync(q.OrderBy(f => f.DepartureTime).ThenBy(f => f.Id), query, cancellationToken);
    }

    public Task<Flight?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        => WithDetails().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<Flight?> GetTrackedWithTagsAsync(int id, CancellationToken cancellationToken = default)
        => Set.Include(f => f.FlightTags)
              .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<bool> HasConfirmedBookingsAsync(int flightId, CancellationToken cancellationToken = default)
        => Context.Bookings.AsNoTracking()
              .AnyAsync(b => b.Status == BookingStatus.Confirmed && b.FlightSeat.FlightId == flightId, cancellationToken);

    public async Task RemoveCancelledBookingsAsync(int flightId, CancellationToken cancellationToken = default)
    {
        // The seats are loaded (tracked) too, so EF orders the DELETEs booking -> seat -> flight.
        var bookings = await Context.Bookings
            .Include(b => b.FlightSeat)
            .Where(b => b.Status == BookingStatus.Cancelled && b.FlightSeat.FlightId == flightId)
            .ToListAsync(cancellationToken);

        Context.Bookings.RemoveRange(bookings);
    }

    public Task<bool> FlightNumberExistsAsync(string flightNumber, int? excludingFlightId, CancellationToken cancellationToken = default)
        => Set.AsNoTracking()
              .AnyAsync(f => f.FlightNumber == flightNumber && (excludingFlightId == null || f.Id != excludingFlightId), cancellationToken);

    public async Task<Dictionary<string, Tag>> GetOrCreateTagsAsync(
        IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var normalized = names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existing = await Context.Tags
            .Where(t => normalized.Contains(t.Name))
            .ToListAsync(cancellationToken);

        var result = existing.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in normalized.Where(n => !result.ContainsKey(n)))
        {
            var tag = new Tag { Name = name };
            await Context.Tags.AddAsync(tag, cancellationToken);
            result[name] = tag;
        }

        return result;
    }
}
