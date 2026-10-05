using System.ComponentModel.DataAnnotations;

namespace FlightBookingSystem.Core.DTOs.Common;

/// <summary>
/// Standard paging parameters bound from the query string. The service layer translates these into
/// database-side <c>Skip</c>/<c>Take</c> so that only one page of rows is materialized.
/// </summary>
public class PaginationQuery
{
    private const int MaxPageSize = 100;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize { get; set; } = 20;

    public int Skip => (Page - 1) * PageSize;

    public int Take => PageSize > MaxPageSize ? MaxPageSize : PageSize;
}
