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

    public async Task<PagedResult<Flight>> SearchAsync(FlightSearchQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<Flight> q = Set.AsNoTracking()
            .Include(f => f.Aircraft)
            .Include(f => f.FlightTags).ThenInclude(ft => ft.Tag);

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

        var totalCount = await q.CountAsync(cancellationToken);

        // Real database-side pagination: Skip/Take are translated to SQL OFFSET/FETCH.
        var items = await q
            .OrderBy(f => f.DepartureTime)
            .ThenBy(f => f.Id)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<Flight>(items, totalCount, query.Page, query.PageSize);
    }

    public Task<Flight?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        => Set.AsNoTracking()
              .Include(f => f.Aircraft)
              .Include(f => f.FlightTags).ThenInclude(ft => ft.Tag)
              .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<Flight?> GetTrackedWithTagsAsync(int id, CancellationToken cancellationToken = default)
        => Set.Include(f => f.FlightTags)
              .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<bool> HasConfirmedBookingsAsync(int flightId, CancellationToken cancellationToken = default)
        => Context.Bookings.AsNoTracking()
              .AnyAsync(b => b.Status == BookingStatus.Confirmed && b.FlightSeat.FlightId == flightId, cancellationToken);

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
