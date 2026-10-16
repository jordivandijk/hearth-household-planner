using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace HouseholdPlanner.Web.Auth;

public class HouseholdAuthStateProvider(HttpClient httpClient, IJSRuntime js) : AuthenticationStateProvider
{
    private const string TokenKey = "hearth_token";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

            if (string.IsNullOrWhiteSpace(token))
                return Unauthenticated();

            var claims = ParseClaims(token);
            if (claims is null)
                return Unauthenticated();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return Unauthenticated();
        }
    }

    public async Task NotifyLoginAsync(string token)
    {
        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var claims = ParseClaims(token) ?? [];
        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task NotifyLogoutAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Unauthenticated()));
    }

    public async Task<string?> GetTokenAsync()
        => await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    /// <summary>
    /// Sends the stored token from now on when it isn't the one the API <paramref name="rejected"/>: a newer one from
    /// a password change. False when there's no other token, so the session is over.
    /// </summary>
    public async Task<bool> UseStoredTokenAsync(string? rejected)
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token) || token == rejected)
            return false;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return true;
    }

    private static AuthenticationState Unauthenticated()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));

    // Manually decode the JWT payload — avoids System.IdentityModel.Tokens.Jwt
    // which has WASM trimming issues in some configurations.
    private static IEnumerable<Claim>? ParseClaims(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            payload = (payload.Length % 4) switch
            {
                2 => payload + "==",
                3 => payload + "=",
                _ => payload
            };

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check expiry before returning claims
            if (root.TryGetProperty("exp", out var expProp))
            {
                var expiry = DateTimeOffset.FromUnixTimeSeconds(expProp.GetInt64()).UtcDateTime;
                if (expiry < DateTime.UtcNow)
                    return null;
            }

            var claims = new List<Claim>();
            foreach (var prop in root.EnumerateObject())
                claims.Add(new Claim(prop.Name, prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString()!
                    : prop.Value.ToString()));

            return claims;
        }
        catch
        {
            return null;
        }
    }
}
