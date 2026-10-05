namespace FlightBookingSystem.Core.Constants;

/// <summary>
/// Central definition of the authorization roles used across the system.
/// Roles are stored on <see cref="Entities.User.Role"/> and emitted as a JWT claim.
/// </summary>
public static class Roles
{
    public const string User = "User";
    public const string Admin = "Admin";
}
