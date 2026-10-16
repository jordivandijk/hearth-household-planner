using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services.Interfaces;

namespace HouseholdPlanner.Api.Services.Implementations;

public class AuthService(
    IUserRepository userRepository, IHouseholdRepository householdRepository, ITokenService tokenService, SignInLimiter limiter)
    : IAuthService
{
    public async Task<ValueResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        if (!limiter.TrySignIn(request.Username))
            return TooManyAttempts();

        var result = await userRepository.GetByUsernameAsync(request.Username.Trim());

        if (!result.Success || !BCrypt.Net.BCrypt.Verify(request.Password, result.Data!.PasswordHash))
            return new("Invalid username or password.", false, new(), ErrorCode.Unauthorized);

        return new("Login successful.", true, ToResponse(result.Data!));
    }

    public async Task<ValueResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (!limiter.TrySignUp())
            return TooManyAttempts();

        var household = await householdRepository.GetByInviteCodeAsync(request.InviteCode);
        if (!household.Success)
            return new(household.Message, false, new(), household.ErrorCode);

        var username = request.Username.Trim();
        if (username.Length < 2)
            return new("Your name needs at least 2 characters.", false, new(), ErrorCode.InvalidInput);
        if (await userRepository.ExistsAsync(username))
            return new("Username is already taken.", false, new(), ErrorCode.DuplicateEntry);

        var members = await userRepository.GetByHouseholdAsync(household.Data!.Id);
        var created = await userRepository.CreateAsync(new User
        {
            Id           = Guid.NewGuid(),
            Username     = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = UserRole.Member,
            Hue          = MemberHues.Next(members.Select(m => m.Hue).ToList()),
            HouseholdId  = household.Data.Id
        });
        if (!created.Success)
            return new(created.Message, false, new(), created.ErrorCode);

        return new("Account created.", true, ToResponse(created.Data!));
    }

    private static ValueResult<AuthResponse> TooManyAttempts()
        => new("Too many attempts. Wait a few minutes and try again.", false, new(), ErrorCode.TooManyRequests);

    private AuthResponse ToResponse(User user) => new()
    {
        Token    = tokenService.GenerateToken(user),
        Username = user.Username,
        Role     = user.Role
    };
}
