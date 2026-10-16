using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Services.Interfaces;

public interface IUserService
{
    Task<ValueResult<UserResponse>> GetByIdAsync(Guid id);
    /// <summary>Signs the account out on every other device, and returns a new token for this one.</summary>
    Task<ValueResult<AuthResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<Result> SetLanguageAsync(Guid userId, SetLanguageRequest request);
    /// <summary>An admin removes someone from their household, which deletes the account.</summary>
    Task<Result> RemoveMemberAsync(Guid householdId, Guid adminId, Guid memberId);
}
