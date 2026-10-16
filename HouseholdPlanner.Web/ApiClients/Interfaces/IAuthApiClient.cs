using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Interfaces;

public interface IAuthApiClient
{
    /// <summary>
    /// Returns null when the credentials are wrong; throws <see cref="ApiException"/> for other failures, such as
    /// too many attempts.
    /// </summary>
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<UserProfile> GetMeAsync();
    /// <summary>Signs the account out on every other device; the response has a new token for this one.</summary>
    Task<AuthResponse> ChangePasswordAsync(ChangePasswordRequest request);
    Task SetLanguageAsync(string language);
}
