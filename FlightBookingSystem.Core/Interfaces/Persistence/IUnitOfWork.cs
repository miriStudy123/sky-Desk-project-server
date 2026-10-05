namespace FlightBookingSystem.Core.Interfaces.Persistence;

/// <summary>
/// Commits the changes tracked by the repositories as a single unit.
/// The concrete implementation wraps <c>DbContext.SaveChangesAsync</c>.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes.
    /// May throw <c>DbUpdateConcurrencyException</c> when a tracked <see cref="Entities.FlightSeat"/>
    /// was modified by another transaction since it was loaded.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
