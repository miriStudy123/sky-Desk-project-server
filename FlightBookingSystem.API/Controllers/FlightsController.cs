using FlightBookingSystem.Core.Constants;
using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.DTOs.Flights;
using FlightBookingSystem.Core.DTOs.Seats;
using FlightBookingSystem.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookingSystem.API.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;
    private readonly ISeatService _seatService;

    public FlightsController(IFlightService flightService, ISeatService seatService)
    {
        _flightService = flightService;
        _seatService = seatService;
    }

    /// <summary>Searches flights with database-side pagination and optional filters.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<FlightResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FlightResponse>>> Search(
        [FromQuery] FlightSearchQuery query, CancellationToken cancellationToken)
        => Ok(await _flightService.SearchAsync(query, cancellationToken));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightResponse>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _flightService.GetByIdAsync(id, cancellationToken));

    [HttpGet("{flightId:int}/seats")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<SeatResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SeatResponse>>> GetSeats(int flightId, CancellationToken cancellationToken)
        => Ok(await _seatService.GetSeatsForFlightAsync(flightId, cancellationToken));

    [HttpGet("{flightId:int}/seats/available")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<SeatResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SeatResponse>>> GetAvailableSeats(int flightId, CancellationToken cancellationToken)
        => Ok(await _seatService.GetAvailableSeatsForFlightAsync(flightId, cancellationToken));

    /// <summary>Creates a flight and its per-flight seat map. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FlightResponse>> Create(CreateFlightRequest request, CancellationToken cancellationToken)
    {
        var flight = await _flightService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = flight.Id }, flight);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightResponse>> Update(int id, UpdateFlightRequest request, CancellationToken cancellationToken)
        => Ok(await _flightService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _flightService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
