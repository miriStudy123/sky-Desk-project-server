using AutoMapper;
using FlightBookingSystem.Core.DTOs.Aircraft;
using FlightBookingSystem.Core.Interfaces.Persistence;
using FlightBookingSystem.Core.Interfaces.Services;

namespace FlightBookingSystem.Services.Services;

public class AircraftService : IAircraftService
{
    private readonly IAircraftRepository _aircraft;
    private readonly IMapper _mapper;

    public AircraftService(IAircraftRepository aircraft, IMapper mapper)
    {
        _aircraft = aircraft;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AircraftResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var aircraft = await _aircraft.ListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<AircraftResponse>>(aircraft);
    }
}
