using FlightBookingSystem.Core.Interfaces.Security;

namespace FlightBookingSystem.Services.Security;

/// <summary>
/// BCrypt-based password hashing. The work factor is deliberately left at the library default so it
/// can be tuned centrally later. Verification is constant-time inside the BCrypt library.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
