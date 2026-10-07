using AutoMapper;
using FlightBookingSystem.Core.DTOs.Aircraft;
using FlightBookingSystem.Core.DTOs.Bookings;
using FlightBookingSystem.Core.DTOs.Flights;
using FlightBookingSystem.Core.DTOs.Seats;
using FlightBookingSystem.Core.DTOs.Users;
using FlightBookingSystem.Core.Entities;

namespace FlightBookingSystem.API.Profiles;

/// <summary>
/// AutoMapper configuration for Entity -> Response DTO mapping. Request DTOs are mapped to entities
/// by hand inside the services, where business defaults (roles, status, timestamps) are applied.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponse>();

        CreateMap<Aircraft, AircraftResponse>();

        CreateMap<Flight, FlightResponse>()
            .ForMember(d => d.AircraftModel, o => o.MapFrom(s => s.Aircraft != null ? s.Aircraft.Model : string.Empty))
            .ForMember(d => d.Tags, o => o.MapFrom(s => s.FlightTags.Select(ft => ft.Tag.Name).ToList()))
            // AvailableSeats is filled in by FlightService from a single grouped query (avoids N+1).
            .ForMember(d => d.AvailableSeats, o => o.Ignore());

        CreateMap<FlightSeat, SeatResponse>()
            .ForMember(d => d.FlightSeatId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.RowNumber, o => o.MapFrom(s => s.Seat.RowNumber))
            .ForMember(d => d.SeatLetter, o => o.MapFrom(s => s.Seat.SeatLetter));

        // Same-named members (Id, Reference, FlightSeatId, BookingDate) and enum -> string Status map by convention.
        CreateMap<Booking, BookingResponse>()
            .ForMember(d => d.FlightId, o => o.MapFrom(s => s.FlightSeat.FlightId))
            .ForMember(d => d.FlightNumber, o => o.MapFrom(s => s.FlightSeat.Flight.FlightNumber))
            .ForMember(d => d.Origin, o => o.MapFrom(s => s.FlightSeat.Flight.Origin))
            .ForMember(d => d.Destination, o => o.MapFrom(s => s.FlightSeat.Flight.Destination))
            .ForMember(d => d.DepartureTime, o => o.MapFrom(s => s.FlightSeat.Flight.DepartureTime))
            .ForMember(d => d.RowNumber, o => o.MapFrom(s => s.FlightSeat.Seat.RowNumber))
            .ForMember(d => d.SeatLetter, o => o.MapFrom(s => s.FlightSeat.Seat.SeatLetter));
    }
}
