using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Enums;
using FlightBookingSystem.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingSystem.Data.Repositories;

public class FlightSeatRepository : Repository<FlightSeat>, IFlightSeatRepository
{
    public FlightSeatRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<FlightSeat>> GetByFlightAsync(
        int flightId, bool onlyAvailable, CancellationToken cancellationToken = default)
    {
        IQueryable<FlightSeat> q = Set.AsNoTracking()
            .Include(fs => fs.Seat)
            .Where(fs => fs.FlightId == flightId);

        if (onlyAvailable)
            q = q.Where(fs => fs.Status == SeatStatus.Available);

        return await q
            .OrderBy(fs => fs.Seat.RowNumber)
            .ThenBy(fs => fs.Seat.SeatLetter)
            .ToListAsync(cancellationToken);
    }

    public Task<FlightSeat?> GetTrackedForBookingAsync(int flightSeatId, CancellationToken cancellationToken = default)
        => Set.Include(fs => fs.Flight)
              .Include(fs => fs.Seat)
              .FirstOrDefaultAsync(fs => fs.Id == flightSeatId, cancellationToken);

    public Task<bool> FlightExistsAsync(int flightId, CancellationToken cancellationToken = default)
        => Context.Flights.AsNoTracking().AnyAsync(f => f.Id == flightId, cancellationToken);

    public async Task<IReadOnlyDictionary<int, int>> GetAvailableSeatCountsAsync(
        IReadOnlyCollection<int> flightIds, CancellationToken cancellationToken = default)
    {
        if (flightIds.Count == 0)
            return new Dictionary<int, int>();

        var counts = await Set.AsNoTracking()
            .Where(fs => flightIds.Contains(fs.FlightId) && fs.Status == SeatStatus.Available)
            .GroupBy(fs => fs.FlightId)
            .Select(g => new { FlightId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = flightIds.ToDictionary(id => id, _ => 0);
        foreach (var row in counts)
            result[row.FlightId] = row.Count;

        return result;
    }
}
