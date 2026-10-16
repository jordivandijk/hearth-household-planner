using System.Net;
using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Implementations;

public class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            return await httpClient.SendJsonAsync<AuthResponse>(HttpMethod.Post, "api/auth/login", request);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request)
        => httpClient.SendJsonAsync<AuthResponse>(HttpMethod.Post, "api/auth/register", request);

    public Task<UserProfile> GetMeAsync()
        => httpClient.GetJsonAsync<UserProfile>("api/users/me");

    public Task<AuthResponse> ChangePasswordAsync(ChangePasswordRequest request)
        => httpClient.SendJsonAsync<AuthResponse>(HttpMethod.Put, "api/users/me/password", request);

    public Task SetLanguageAsync(string language)
        => httpClient.SendJsonAsync(HttpMethod.Put, "api/users/me/language", new SetLanguageRequest { Language = language });
}
