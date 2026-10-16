using HouseholdPlanner.Api.Models;
using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services.Interfaces;

namespace HouseholdPlanner.Api.Services.Implementations;

public class UserService(IUserRepository userRepository, ITokenService tokenService) : IUserService
{
    public async Task<ValueResult<UserResponse>> GetByIdAsync(Guid id)
    {
        var result = await userRepository.GetByIdAsync(id);
        if (!result.Success)
            return new(result.Message, false, new(), result.ErrorCode);

        var user = result.Data!;
        return new("User found.", true, new UserResponse
        {
            Id          = user.Id,
            Username    = user.Username,
            Role        = user.Role,
            Hue         = user.Hue,
            Language    = user.Language,
            HouseholdId = user.HouseholdId
        });
    }

    public async Task<ValueResult<AuthResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var result = await userRepository.GetByIdAsync(userId);
        if (!result.Success)
            return new(result.Message, false, new(), result.ErrorCode);

        var user = result.Data!;
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return new("Current password is incorrect.", false, new(), ErrorCode.InvalidInput);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        // Signs the account out everywhere else: their tokens carry the old stamp. This device gets a new token.
        user.SecurityStamp = Guid.NewGuid();
        var saved = await userRepository.UpdateAsync(user);
        if (!saved.Success)
            return new(saved.Message, false, new(), saved.ErrorCode);

        return new("Password changed.", true, new AuthResponse
        {
            Token    = tokenService.GenerateToken(user),
            Username = user.Username,
            Role     = user.Role
        });
    }

    public async Task<Result> SetLanguageAsync(Guid userId, SetLanguageRequest request)
    {
        if (!Languages.IsSupported(request.Language))
            return new("That language isn't available.", false, ErrorCode.InvalidInput);

        var result = await userRepository.GetByIdAsync(userId);
        if (!result.Success)
            return new(result.Message, false, result.ErrorCode);

        result.Data!.Language = request.Language;
        return await userRepository.UpdateAsync(result.Data);
    }

    public async Task<Result> RemoveMemberAsync(Guid householdId, Guid adminId, Guid memberId)
    {
        // Also keeps a household from losing its last admin.
        if (memberId == adminId)
            return new("You can't remove your own account.", false, ErrorCode.InvalidInput);

        var result = await userRepository.GetByIdAsync(memberId);
        if (!result.Success || result.Data!.HouseholdId != householdId)
            return new("That person isn't in your household.", false, ErrorCode.NotFound);

        // Their sessions end on their next request: tokens are checked against the database.
        await userRepository.DeleteAsync(result.Data);
        return new($"{result.Data.Username} was removed.", true);
    }
}
