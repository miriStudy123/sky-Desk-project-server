using FlightBookingSystem.Core.Entities;

namespace FlightBookingSystem.Core.Interfaces.Security;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
