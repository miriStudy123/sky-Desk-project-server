using AutoMapper;
using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.DTOs.Flights;
using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Enums;
using FlightBookingSystem.Core.Exceptions;
using FlightBookingSystem.Core.Interfaces.Persistence;
using FlightBookingSystem.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FlightBookingSystem.Services.Services;

public class FlightService : IFlightService
{
    private readonly IFlightRepository _flights;
    private readonly IFlightSeatRepository _flightSeats;
    private readonly IAircraftRepository _aircraft;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FlightService> _logger;

    public FlightService(
        IFlightRepository flights,
        IFlightSeatRepository flightSeats,
        IAircraftRepository aircraft,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<FlightService> logger)
    {
        _flights = flights;
        _flightSeats = flightSeats;
        _aircraft = aircraft;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<FlightResponse>> SearchAsync(FlightSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = await _flights.SearchAsync(query, cancellationToken);

        var flightIds = page.Items.Select(f => f.Id).ToList();
        var availability = await _flightSeats.GetAvailableSeatCountsAsync(flightIds, cancellationToken);

        var items = page.Items.Select(f => ToResponse(f, availability.GetValueOrDefault(f.Id))).ToList();

        return new PagedResult<FlightResponse>(items, page.TotalCount, page.Page, page.PageSize);
    }
    
    public async Task<FlightResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var flight = await _flights.GetWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), id);

        var availability = await _flightSeats.GetAvailableSeatCountsAsync(new[] { id }, cancellationToken);
        return ToResponse(flight, availability.GetValueOrDefault(id));
    }

    public async Task<FlightResponse> CreateAsync(CreateFlightRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, null, cancellationToken);

        var aircraft = await _aircraft.GetWithSeatsAsync(request.AircraftId, cancellationToken)
            ?? throw new NotFoundException(nameof(Aircraft), request.AircraftId);

        var flight = new Flight { AircraftId = aircraft.Id };

        // Materialize one bookable FlightSeat per physical seat on the aircraft.
        foreach (var seat in aircraft.Seats)
            flight.FlightSeats.Add(new FlightSeat { SeatId = seat.Id, Status = SeatStatus.Available });

        await ApplyDetailsAsync(flight, request, cancellationToken);

        await _flights.AddAsync(flight, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight created. FlightId={FlightId} Number={FlightNumber}", flight.Id, flight.FlightNumber);

        return await GetByIdAsync(flight.Id, cancellationToken);
    }

    public async Task<FlightResponse> UpdateAsync(int id, UpdateFlightRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, id, cancellationToken);

        var flight = await _flights.GetTrackedWithTagsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), id);

        await ApplyDetailsAsync(flight, request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight updated. FlightId={FlightId}", flight.Id);

        return await GetByIdAsync(flight.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var flight = await _flights.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), id);

        if (await _flights.HasConfirmedBookingsAsync(id, cancellationToken))
            throw new BusinessRuleException("The flight has confirmed bookings and cannot be deleted.");

        await _flights.RemoveCancelledBookingsAsync(id, cancellationToken);
        _flights.Remove(flight);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight deleted. FlightId={FlightId}", id);
    }

    private async Task ValidateAsync(UpdateFlightRequest request, int? excludingFlightId, CancellationToken cancellationToken)
    {
        if (request.ArrivalTime <= request.DepartureTime)
            throw new BusinessRuleException("Arrival time must be after departure time.");

        if (await _flights.FlightNumberExistsAsync(request.FlightNumber, excludingFlightId, cancellationToken))
            throw new BusinessRuleException($"Flight number '{request.FlightNumber}' is already in use.");
    }

    /// <summary>Copies the editable fields (shared by create and update) onto the flight and replaces its tags.</summary>
    private async Task ApplyDetailsAsync(Flight flight, UpdateFlightRequest request, CancellationToken cancellationToken)
    {
        flight.FlightNumber = request.FlightNumber.Trim();
        flight.Origin = request.Origin.Trim();
        flight.Destination = request.Destination.Trim();
        flight.DepartureTime = request.DepartureTime;
        flight.ArrivalTime = request.ArrivalTime;

        flight.FlightTags.Clear();
        if (request.Tags is not { Count: > 0 })
            return;

        // GetOrCreateTagsAsync trims, drops blanks and de-duplicates the names.
        var tags = await _flights.GetOrCreateTagsAsync(request.Tags, cancellationToken);
        foreach (var tag in tags.Values)
            flight.FlightTags.Add(new FlightTag { Tag = tag });
    }

    private FlightResponse ToResponse(Flight flight, int availableSeats)
    {
        var response = _mapper.Map<FlightResponse>(flight);
        response.AvailableSeats = availableSeats;
        return response;
    }
}
