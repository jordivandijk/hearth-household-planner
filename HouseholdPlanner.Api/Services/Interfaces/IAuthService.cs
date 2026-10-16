using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Services.Interfaces;

public interface IAuthService
{
    Task<ValueResult<AuthResponse>> LoginAsync(LoginRequest request);
    /// <summary>Creates an account in the household of the invite code and signs it in.</summary>
    Task<ValueResult<AuthResponse>> RegisterAsync(RegisterRequest request);
}
