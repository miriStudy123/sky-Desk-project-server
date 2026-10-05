namespace FlightBookingSystem.Core.Interfaces.Security;

/// <summary>
/// Abstraction over the authenticated principal for the current request.
/// Implemented in the API layer on top of <c>IHttpContextAccessor</c> so that services stay
/// free of any dependency on ASP.NET Core.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    int UserId { get; }

    string? Email { get; }

    bool IsAdmin { get; }
}
