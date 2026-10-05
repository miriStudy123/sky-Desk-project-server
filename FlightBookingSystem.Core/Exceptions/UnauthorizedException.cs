namespace FlightBookingSystem.Core.Exceptions;

/// <summary>
/// Thrown when authentication fails (bad credentials) or a caller is not allowed to act on a
/// resource they do not own. Translated to HTTP 401 / 403 by <c>ExceptionHandlingMiddleware</c>.
/// </summary>
public class UnauthorizedException : Exception
{
    public bool IsForbidden { get; }

    public UnauthorizedException(string message, bool isForbidden = false) : base(message)
        => IsForbidden = isForbidden;
}
