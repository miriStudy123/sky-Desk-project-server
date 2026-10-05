using FlightBookingSystem.Core.Constants;

namespace FlightBookingSystem.Core.Entities;

/// <summary>
/// An application user. Passwords are never stored in clear text - only <see cref="PasswordHash"/> is persisted.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    /// <summary>One of <see cref="Roles"/>. Emitted as a role claim in the JWT.</summary>
    public string Role { get; set; } = Roles.User;

    // Navigation - User 1:N Booking
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
