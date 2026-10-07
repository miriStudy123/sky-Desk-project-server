namespace FlightBookingSystem.Core.Interfaces.Persistence;

/// <summary>
/// Generic data-access contract. Implementations live in the Data layer and must not contain
/// business logic - only querying and change tracking.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Remove(T entity);
}
