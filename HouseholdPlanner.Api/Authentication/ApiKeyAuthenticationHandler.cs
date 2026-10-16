using System.Security.Claims;
using System.Text.Encodings.Web;
using HouseholdPlanner.Api.Data;
using HouseholdPlanner.Api.Services;
using HouseholdPlanner.Api.Services.Implementations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace HouseholdPlanner.Api.Authentication;

/// <summary>
/// Signs in an automation that sends "Authorization: Bearer hearth_…" as the admin who made that key, with the same
/// claims a sign-in token has, so every endpoint works the same for both.
/// </summary>
public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    AppDbContext db, TimeProvider time)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";

    /// <summary>Last used is saved at most this often, so an automation that polls doesn't write on every request.</summary>
    private static readonly TimeSpan LastUsedPrecision = TimeSpan.FromMinutes(1);

    /// <summary>The key in the request, if it has one.</summary>
    public static string? KeyFrom(HttpRequest request)
    {
        var header = request.Headers[HeaderNames.Authorization].ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;
        var token = header["Bearer ".Length..].Trim();
        return ApiKeySecrets.IsKey(token) ? token : null;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (KeyFrom(Request) is not { } secret)
            return AuthenticateResult.NoResult();

        var hash = ApiKeySecrets.Hash(secret);
        var key = await db.ApiKeys.AsNoTracking()
            .Where(k => k.Hash == hash)
            .Select(k => new { k.Id, k.LastUsedAt, k.UserId, k.User.Username, k.User.Role, k.User.HouseholdId })
            .FirstOrDefaultAsync(Context.RequestAborted);
        if (key is null)
            return AuthenticateResult.Fail("Unknown or revoked API key.");

        var now = time.GetUtcNow().UtcDateTime;
        if (key.LastUsedAt is null || now - key.LastUsedAt >= LastUsedPrecision)
            await db.ApiKeys.Where(k => k.Id == key.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(k => k.LastUsedAt, now), Context.RequestAborted);

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, key.UserId.ToString()),
            new Claim(ClaimTypes.Name, key.Username),
            new Claim(ClaimTypes.Role, key.Role.ToString()),
            new Claim(TokenService.HouseholdClaim, key.HouseholdId.ToString())
        ], SchemeName, ClaimTypes.Name, ClaimTypes.Role);

        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName));
    }
}
