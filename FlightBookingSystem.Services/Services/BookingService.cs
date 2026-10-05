using AutoMapper;
using FlightBookingSystem.Core.DTOs.Bookings;
using FlightBookingSystem.Core.DTOs.Common;
using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Enums;
using FlightBookingSystem.Core.Exceptions;
using FlightBookingSystem.Core.Interfaces.Persistence;
using FlightBookingSystem.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightBookingSystem.Services.Services;

/// <summary>
/// Business core of the system. Every state-dependent decision for a booking - existence, seat/flight
/// match, availability, duplicate protection, the status transition and the save - happens here as one
/// guarded unit, protected by the optimistic-concurrency token on <see cref="FlightSeat"/>.
/// </summary>
public class BookingService : IBookingService
{
    /// <summary>A booking (and a cancellation) is not allowed once the flight is within this window.</summary>
    private static readonly TimeSpan CutoffBeforeDeparture = TimeSpan.FromHours(2);

    private readonly IFlightSeatRepository _flightSeats;
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IFlightSeatRepository flightSeats,
        IBookingRepository bookings,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<BookingService> logger)
    {
        _flightSeats = flightSeats;
        _bookings = bookings;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateAsync(int userId, CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var flightSeat = await _flightSeats.GetTrackedForBookingAsync(request.FlightSeatId, cancellationToken)
            ?? throw new NotFoundException(nameof(FlightSeat), request.FlightSeatId);

        if (flightSeat.Flight.DepartureTime - DateTime.UtcNow <= CutoffBeforeDeparture)
            throw new BusinessRuleException("This flight is too close to departure to accept new bookings.");

        if (flightSeat.Status != SeatStatus.Available)
        {
            _logger.LogWarning(
                "Booking rejected - seat not available. FlightSeatId={FlightSeatId} Status={Status} UserId={UserId}",
                flightSeat.Id, flightSeat.Status, userId);
            throw new ConflictException("The selected seat is no longer available. Please choose another seat.");
        }

        if (await _bookings.ActiveBookingExistsForSeatAsync(flightSeat.Id, cancellationToken))
            throw new ConflictException("The selected seat is no longer available. Please choose another seat.");

        // Atomic unit: flip the status and insert the booking, then let the concurrency token arbitrate.
        flightSeat.Status = SeatStatus.Booked;
        _flightSeats.Update(flightSeat);

        var booking = new Booking
        {
            UserId = userId,
            FlightSeatId = flightSeat.Id,
            BookingDate = DateTime.UtcNow,
            Status = BookingStatus.Confirmed,
            Reference = GenerateReference()
        };
        await _bookings.AddAsync(booking, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Another request updated this exact FlightSeat between our read and our write.
            _logger.LogWarning(ex,
                "Concurrency conflict booking seat. FlightSeatId={FlightSeatId} UserId={UserId}",
                flightSeat.Id, userId);
            throw new ConflictException("Someone just booked this seat. Please choose another seat.");
        }

        _logger.LogInformation(
            "Seat booked. BookingId={BookingId} FlightSeatId={FlightSeatId} UserId={UserId}",
            booking.Id, flightSeat.Id, userId);

        var created = await _bookings.GetWithDetailsAsync(booking.Id, cancellationToken);
        return _mapper.Map<BookingResponse>(created);
    }

    public async Task<PagedResult<BookingResponse>> GetMyBookingsAsync(int userId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var page = await _bookings.GetForUserAsync(userId, query, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<BookingResponse>>(page.Items);
        return new PagedResult<BookingResponse>(items, page.TotalCount, page.Page, page.PageSize);
    }

    public async Task<BookingResponse> GetByIdAsync(int userId, bool isAdmin, int bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Booking), bookingId);

        EnsureCanAccess(booking, userId, isAdmin);
        return _mapper.Map<BookingResponse>(booking);
    }

    public async Task CancelAsync(int userId, bool isAdmin, int bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Booking), bookingId);

        EnsureCanAccess(booking, userId, isAdmin);

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessRuleException("The booking is already cancelled.");

        if (!isAdmin && booking.FlightSeat.Flight.DepartureTime - DateTime.UtcNow <= CutoffBeforeDeparture)
            throw new BusinessRuleException("The flight is too close to departure to cancel this booking.");

        booking.Status = BookingStatus.Cancelled;
        booking.FlightSeat.Status = SeatStatus.Available;

        _bookings.Update(booking);
        _flightSeats.Update(booking.FlightSeat);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict cancelling booking. BookingId={BookingId}", bookingId);
            throw new ConflictException("The booking was modified concurrently. Please retry.");
        }

        _logger.LogInformation("Booking cancelled. BookingId={BookingId} UserId={UserId}", bookingId, userId);
    }

    private static string GenerateReference()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return string.Create(8, alphabet, static (span, alpha) =>
        {
            for (var i = 0; i < span.Length; i++)
                span[i] = alpha[Random.Shared.Next(alpha.Length)];
        });
    }

    private static void EnsureCanAccess(Booking booking, int userId, bool isAdmin)
    {
        if (!isAdmin && booking.UserId != userId)
            throw new UnauthorizedException("You are not allowed to access this booking.", isForbidden: true);
    }
}
