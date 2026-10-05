using FlightBookingSystem.Core.DTOs.Bookings;
using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.Interfaces.Security;
using FlightBookingSystem.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookingSystem.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ICurrentUser _currentUser;

    public BookingsController(IBookingService bookingService, ICurrentUser currentUser)
    {
        _bookingService = bookingService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Books a seat for the current user. Returns 409 when the seat was taken by a concurrent request.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CreateAsync(_currentUser.UserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResult<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetMy(
        [FromQuery] PaginationQuery query, CancellationToken cancellationToken)
        => Ok(await _bookingService.GetMyBookingsAsync(_currentUser.UserId, query, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _bookingService.GetByIdAsync(_currentUser.UserId, _currentUser.IsAdmin, id, cancellationToken));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _bookingService.CancelAsync(_currentUser.UserId, _currentUser.IsAdmin, id, cancellationToken);
        return NoContent();
    }
}
