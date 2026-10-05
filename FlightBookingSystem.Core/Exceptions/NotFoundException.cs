namespace FlightBookingSystem.Core.Exceptions;

/// <summary>
/// Thrown by the service layer when a requested resource does not exist.
/// Translated to HTTP 404 by <c>ExceptionHandlingMiddleware</c>.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resource, object key)
        : base($"{resource} with key '{key}' was not found.") { }
}
