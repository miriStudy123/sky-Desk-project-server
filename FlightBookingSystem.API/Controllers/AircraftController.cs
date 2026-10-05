using FlightBookingSystem.Core.Constants;
using FlightBookingSystem.Core.DTOs.Aircraft;
using FlightBookingSystem.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookingSystem.API.Controllers;

/// <summary>Read-only lookup used by the admin UI when creating or editing a flight.</summary>
[ApiController]
[Route("api/aircraft")]
[Authorize(Roles = Roles.Admin)]
public class AircraftController : ControllerBase
{
    private readonly IAircraftService _aircraftService;

    public AircraftController(IAircraftService aircraftService) => _aircraftService = aircraftService;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AircraftResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AircraftResponse>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _aircraftService.GetAllAsync(cancellationToken));
}
