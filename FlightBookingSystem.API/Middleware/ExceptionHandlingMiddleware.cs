using System.Text.Json;
using FlightBookingSystem.Core.Exceptions;

namespace FlightBookingSystem.API.Middleware;

/// <summary>
/// Global error boundary. Sits early in the pipeline and converts any unhandled exception into a
/// uniform JSON <c>ProblemDetails</c> response. Domain exceptions map to specific status codes;
/// anything else becomes a 500 without leaking internal detail.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var value)
            ? value?.ToString()
            : context.TraceIdentifier;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? "-"
        });

        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violation"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            UnauthorizedException { IsForbidden: true } => (StatusCodes.Status403Forbidden, "Forbidden"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);
        else
            _logger.LogWarning("Handled {ExceptionType}: {Message}. CorrelationId={CorrelationId}",
                exception.GetType().Name, exception.Message, correlationId);

        var problem = new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title,
            status = statusCode,
            detail = statusCode >= 500 && !_environment.IsDevelopment()
                ? "Please contact support and quote the correlation id."
                : exception.Message,
            correlationId
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
