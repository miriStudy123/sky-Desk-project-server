using FlightBookingSystem.Core.DTOs.Users;

namespace FlightBookingSystem.Core.DTOs.Auth;

/// <summary>
/// Returned by register and login. Contains the signed JWT and basic profile information.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public UserResponse User { get; set; } = null!;
}
