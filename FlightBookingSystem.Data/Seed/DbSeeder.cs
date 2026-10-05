using FlightBookingSystem.Core.Constants;
using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Enums;
using FlightBookingSystem.Core.Interfaces.Security;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingSystem.Data.Seed;

/// <summary>
/// Idempotent demo-data seeder. Applies pending migrations and then inserts a base data set
/// (admin + regular users, aircraft with a full seat map, flights with per-flight seats, and tags)
/// only when the database is empty.
/// </summary>
public static class DbSeeder
{
    public const string AdminEmail = "admin@flightbooking.local";
    public const string AdminPassword = "Admin123!";
    public const string UserEmail = "user@flightbooking.local";
    public const string UserPassword = "User123!";

    public static async Task SeedAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Users.AnyAsync(cancellationToken))
            return;

        // --- Users ---
        var users = new List<User>
        {
            new() { Name = "System Admin", Email = AdminEmail, Role = Roles.Admin, PasswordHash = passwordHasher.Hash(AdminPassword) },
            new() { Name = "Demo User", Email = UserEmail, Role = Roles.User, PasswordHash = passwordHasher.Hash(UserPassword) },
            new() { Name = "Dana Levi", Email = "dana@example.com", Role = Roles.User, PasswordHash = passwordHasher.Hash("Passw0rd!") },
            new() { Name = "Yossi Cohen", Email = "yossi@example.com", Role = Roles.User, PasswordHash = passwordHasher.Hash("Passw0rd!") },
        };
        await context.Users.AddRangeAsync(users, cancellationToken);

        // --- Tags ---
        var tagDirect = new Tag { Name = "Direct" };
        var tagDomestic = new Tag { Name = "Domestic" };
        var tagRedEye = new Tag { Name = "RedEye" };
        await context.Tags.AddRangeAsync(new[] { tagDirect, tagDomestic, tagRedEye }, cancellationToken);

        // --- Aircraft + seat map ---
        var narrowBody = BuildAircraft("Airbus A320", rows: 30, letters: "ABCDEF");
        var regional = BuildAircraft("Embraer E190", rows: 20, letters: "ABCD");
        await context.Aircraft.AddRangeAsync(new[] { narrowBody, regional }, cancellationToken);

        // --- Flights + per-flight seats ---
        var today = DateTime.UtcNow.Date;
        var flights = new List<Flight>
        {
            BuildFlight("FB100", "TLV", "JFK", today.AddDays(3).AddHours(23), 11, narrowBody, new[] { tagRedEye, tagDirect }),
            BuildFlight("FB200", "TLV", "LHR", today.AddDays(2).AddHours(9), 5, narrowBody, new[] { tagDirect }),
            BuildFlight("FB300", "TLV", "ETH", today.AddDays(1).AddHours(7), 1, regional, new[] { tagDomestic, tagDirect }),
            BuildFlight("FB400", "TLV", "CDG", today.AddDays(5).AddHours(14), 5, regional, new[] { tagDirect }),
        };
        await context.Flights.AddRangeAsync(flights, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static Aircraft BuildAircraft(string model, int rows, string letters)
    {
        var aircraft = new Aircraft { Model = model, TotalSeats = rows * letters.Length };
        for (var row = 1; row <= rows; row++)
        {
            foreach (var letter in letters)
                aircraft.Seats.Add(new Seat { RowNumber = row, SeatLetter = letter.ToString() });
        }
        return aircraft;
    }

    private static Flight BuildFlight(
        string number, string origin, string destination,
        DateTime departure, double durationHours, Aircraft aircraft, IEnumerable<Tag> tags)
    {
        var flight = new Flight
        {
            FlightNumber = number,
            Origin = origin,
            Destination = destination,
            DepartureTime = departure,
            ArrivalTime = departure.AddHours(durationHours),
            Aircraft = aircraft
        };

        foreach (var seat in aircraft.Seats)
            flight.FlightSeats.Add(new FlightSeat { Seat = seat, Status = SeatStatus.Available });

        foreach (var tag in tags)
            flight.FlightTags.Add(new FlightTag { Tag = tag });

        return flight;
    }
}
