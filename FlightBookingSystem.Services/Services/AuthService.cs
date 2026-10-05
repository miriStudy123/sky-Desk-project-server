using AutoMapper;
using FlightBookingSystem.Core.Constants;
using FlightBookingSystem.Core.DTOs.Auth;
using FlightBookingSystem.Core.DTOs.Users;
using FlightBookingSystem.Core.Entities;
using FlightBookingSystem.Core.Exceptions;
using FlightBookingSystem.Core.Interfaces.Persistence;
using FlightBookingSystem.Core.Interfaces.Security;
using FlightBookingSystem.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
namespace FlightBookingSystem.Services.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.EmailExistsAsync(email, cancellationToken))
            throw new BusinessRuleException("An account with this email already exists.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            // A newly registered account is always a regular user - the role is never taken from the request.
            Role = Roles.User,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user registered. UserId={UserId}", user.Id);

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("Failed login attempt for unknown email hash {EmailHash}", email.GetHashCode());
            throw new UnauthorizedException("You are not registered in the system. Please create an account.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogInformation("Failed login attempt (wrong password) for email hash {EmailHash}", email.GetHashCode());
            throw new UnauthorizedException("Incorrect password. Passwords must be at least 6 characters long.");
        }

        _logger.LogInformation("User logged in. UserId={UserId}", user.Id);
        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAtUtc) = _tokenGenerator.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = _mapper.Map<UserResponse>(user)
        };
    }
}
