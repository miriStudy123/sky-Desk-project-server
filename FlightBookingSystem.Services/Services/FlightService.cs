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
        ValidateSchedule(request.DepartureTime, request.ArrivalTime);

        if (await _flights.FlightNumberExistsAsync(request.FlightNumber, null, cancellationToken))
            throw new BusinessRuleException($"Flight number '{request.FlightNumber}' is already in use.");

        var aircraft = await _aircraft.GetWithSeatsAsync(request.AircraftId, cancellationToken)
            ?? throw new NotFoundException(nameof(Aircraft), request.AircraftId);

        var flight = new Flight
        {
            FlightNumber = request.FlightNumber.Trim(),
            Origin = request.Origin.Trim(),
            Destination = request.Destination.Trim(),
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            AircraftId = aircraft.Id
        };

        // Materialize one bookable FlightSeat per physical seat on the aircraft.
        foreach (var seat in aircraft.Seats)
            flight.FlightSeats.Add(new FlightSeat { SeatId = seat.Id, Status = SeatStatus.Available });

        await ApplyTagsAsync(flight, request.Tags, cancellationToken);

        await _flights.AddAsync(flight, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight created. FlightId={FlightId} Number={FlightNumber}", flight.Id, flight.FlightNumber);

        return await GetByIdAsync(flight.Id, cancellationToken);
    }

    public async Task<FlightResponse> UpdateAsync(int id, UpdateFlightRequest request, CancellationToken cancellationToken = default)
    {
        ValidateSchedule(request.DepartureTime, request.ArrivalTime);

        var flight = await _flights.GetTrackedWithTagsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), id);

        if (await _flights.FlightNumberExistsAsync(request.FlightNumber, id, cancellationToken))
            throw new BusinessRuleException($"Flight number '{request.FlightNumber}' is already in use.");

        flight.FlightNumber = request.FlightNumber.Trim();
        flight.Origin = request.Origin.Trim();
        flight.Destination = request.Destination.Trim();
        flight.DepartureTime = request.DepartureTime;
        flight.ArrivalTime = request.ArrivalTime;

        flight.FlightTags.Clear();
        await ApplyTagsAsync(flight, request.Tags, cancellationToken);

        _flights.Update(flight);
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

        _flights.Remove(flight);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight deleted. FlightId={FlightId}", id);
    }

    private static void ValidateSchedule(DateTime departure, DateTime arrival)
    {
        if (arrival <= departure)
            throw new BusinessRuleException("Arrival time must be after departure time.");
    }

    private async Task ApplyTagsAsync(Flight flight, IEnumerable<string> tagNames, CancellationToken cancellationToken)
    {
        var names = tagNames?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
        if (names.Count == 0)
            return;

        var tags = await _flights.GetOrCreateTagsAsync(names, cancellationToken);
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
