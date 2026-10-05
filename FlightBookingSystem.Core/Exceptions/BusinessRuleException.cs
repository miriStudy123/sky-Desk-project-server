namespace FlightBookingSystem.Core.Exceptions;

/// <summary>
/// Thrown when a request is well-formed but violates a business rule
/// (for example cancelling a booking after the cancellation window has closed).
/// Translated to HTTP 400 by <c>ExceptionHandlingMiddleware</c>.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
