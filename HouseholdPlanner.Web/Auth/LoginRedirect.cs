using Microsoft.AspNetCore.Components;

namespace HouseholdPlanner.Web.Auth;

/// <summary>
/// Sends the user to the login page and, once signed in, back to the page they were on,
/// e.g. a link someone shared.
/// </summary>
public static class LoginRedirect
{
    public static string LoginUrl(NavigationManager nav, bool expired = false)
    {
        var returnUrl = nav.ToBaseRelativePath(nav.Uri);
        var query = new List<string>();
        if (expired)
            query.Add("expired=true");
        if (returnUrl.Length > 0 && !IsLoginPage(nav))
            query.Add($"returnUrl={Uri.EscapeDataString(returnUrl)}");
        return query.Count == 0 ? "/login" : $"/login?{string.Join("&", query)}";
    }

    public static bool IsLoginPage(NavigationManager nav)
        => new Uri(nav.Uri).AbsolutePath.TrimEnd('/').Equals("/login", StringComparison.OrdinalIgnoreCase);

    /// <summary>Where to go after signing in: the return URL if it's a page of this app, so a crafted link can't send someone elsewhere.</summary>
    public static string Destination(NavigationManager nav, string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl)
           && Uri.TryCreate(new Uri(nav.BaseUri), returnUrl, out var target)
           && target.AbsoluteUri.StartsWith(nav.BaseUri, StringComparison.OrdinalIgnoreCase)
            ? target.AbsoluteUri
            : "/";
}
