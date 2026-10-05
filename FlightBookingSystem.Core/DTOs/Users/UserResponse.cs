namespace FlightBookingSystem.Core.DTOs.Users;

public class UserResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;
}
