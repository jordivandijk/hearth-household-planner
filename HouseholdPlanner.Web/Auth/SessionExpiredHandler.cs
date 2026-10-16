using System.Net;
using HouseholdPlanner.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HouseholdPlanner.Web.Auth;

/// <summary>
/// Signs the user out and sends them to the login page when the API rejects their token
/// (expired, the account was deleted, or its password changed on another device), instead of every page
/// silently rendering empty.
/// </summary>
public class SessionExpiredHandler(IServiceProvider services) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // Only requests that carried a token can have an expired session; a failed login also returns 401.
        if (response.StatusCode == HttpStatusCode.Unauthorized && request.Headers.Authorization is { } sent)
        {
            // Resolved lazily: the auth services themselves depend on this HttpClient.
            // A request that went out with the old token while the password was changed here (or in another tab)
            // fails, but the new token is stored by then and the app stays signed in with it.
            if (await services.GetRequiredService<HouseholdAuthStateProvider>().UseStoredTokenAsync(sent.Parameter))
                return response;

            await services.GetRequiredService<IAuthService>().LogoutAsync();

            // A page's other requests fail the same way; the first one already went to the login page
            // with the way back, which a second redirect from there would lose.
            var nav = services.GetRequiredService<NavigationManager>();
            if (!LoginRedirect.IsLoginPage(nav))
                nav.NavigateTo(LoginRedirect.LoginUrl(nav, expired: true));
        }

        return response;
    }
}
