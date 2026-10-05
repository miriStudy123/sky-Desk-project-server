using System.ComponentModel.DataAnnotations;

namespace FlightBookingSystem.Services.Security;

/// <summary>
/// Strongly-typed JWT settings, bound from the <c>Jwt</c> configuration section.
/// The <see cref="SecretKey"/> must come from User Secrets / environment variables, never source control.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}
