using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingSystem.Data.Repositories;

public class AircraftRepository : Repository<Aircraft>, IAircraftRepository
{
    public AircraftRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Aircraft?> GetWithSeatsAsync(int aircraftId, CancellationToken cancellationToken = default)
        => Set.Include(a => a.Seats)
              .FirstOrDefaultAsync(a => a.Id == aircraftId, cancellationToken);
}
