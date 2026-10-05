namespace FlightBookingSystem.Core.Exceptions;

/// <summary>
/// Thrown when a concurrent change to the scarce resource (a <see cref="Entities.FlightSeat"/>)
/// prevents the current operation from completing - typically after the service layer catches a
/// <c>DbUpdateConcurrencyException</c>. Translated to HTTP 409 by <c>ExceptionHandlingMiddleware</c>.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
