using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingSystem.Data.Repositories;

/// <summary>
/// Generic EF Core repository. Pure data access - no business rules.
/// Read helpers use <c>AsNoTracking</c>; callers that need to mutate an entity use the
/// entity-specific repositories which return tracked instances.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(AppDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Set.FindAsync(new object?[] { id }, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await Set.AddAsync(entity, cancellationToken);

    public virtual void Remove(T entity) => Set.Remove(entity);

    /// <summary>
    /// Real database-side pagination: counts the matching rows, then applies Skip/Take in SQL so only
    /// one page is materialized.
    /// </summary>
    protected static async Task<PagedResult<T>> ToPagedResultAsync(
        IOrderedQueryable<T> query, PaginationQuery paging, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(paging.Skip).Take(paging.Take).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, totalCount, paging.Page, paging.PageSize);
    }
}
