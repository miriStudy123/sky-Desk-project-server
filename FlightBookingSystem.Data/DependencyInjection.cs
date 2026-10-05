using FlightBookingSystem.Core.Interfaces.Persistence;
using FlightBookingSystem.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlightBookingSystem.Data;

/// <summary>
/// Composition root for the Data layer. Registers the <see cref="AppDbContext"/> (SQL Server),
/// the repositories, and the unit of work. Called from the API's <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. " +
                "Set it via User Secrets or environment variables - it must never be committed.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAircraftRepository, AircraftRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IFlightSeatRepository, FlightSeatRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}
