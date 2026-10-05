using System.Security.Claims;
using FlightBookingSystem.Core.Constants;
using FlightBookingSystem.Core.Interfaces.Security;

namespace FlightBookingSystem.API.Security;

/// <summary>
/// Reads the authenticated principal off the current <c>HttpContext</c> so the service layer can stay
/// unaware of ASP.NET Core.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? Principal?.FindFirstValue("sub");
            return int.TryParse(raw, out var id)
                ? id
                : throw new InvalidOperationException("The current request has no authenticated user id.");
        }
    }

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public bool IsAdmin => Principal?.IsInRole(Roles.Admin) ?? false;
}
