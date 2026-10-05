using AutoMapper;
using FlightBookingSystem.Core.DTOs.Seats;
using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Exceptions;
using FlightBookingSystem.Core.Interfaces.Persistence;
using FlightBookingSystem.Core.Interfaces.Services;

namespace FlightBookingSystem.Services.Services;

public class SeatService : ISeatService
{
    private readonly IFlightSeatRepository _flightSeats;
    private readonly IMapper _mapper;

    public SeatService(IFlightSeatRepository flightSeats, IMapper mapper)
    {
        _flightSeats = flightSeats;
        _mapper = mapper;
    }

    public Task<IReadOnlyList<SeatResponse>> GetSeatsForFlightAsync(int flightId, CancellationToken cancellationToken = default)
        => GetSeatsAsync(flightId, onlyAvailable: false, cancellationToken);

    public Task<IReadOnlyList<SeatResponse>> GetAvailableSeatsForFlightAsync(int flightId, CancellationToken cancellationToken = default)
        => GetSeatsAsync(flightId, onlyAvailable: true, cancellationToken);

    private async Task<IReadOnlyList<SeatResponse>> GetSeatsAsync(int flightId, bool onlyAvailable, CancellationToken cancellationToken)
    {
        if (!await _flightSeats.FlightExistsAsync(flightId, cancellationToken))
            throw new NotFoundException(nameof(Flight), flightId);

        var seats = await _flightSeats.GetByFlightAsync(flightId, onlyAvailable, cancellationToken);
        return _mapper.Map<IReadOnlyList<SeatResponse>>(seats);
    }
}
