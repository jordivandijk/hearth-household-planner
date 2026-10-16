using System.Globalization;
using Microsoft.JSInterop;

namespace HouseholdPlanner.Web.Services;

/// <summary>
/// The app's language. It's chosen before the app starts (Blazor can't switch languages while running), from
/// the choice saved on this device, or else the browser's language. The account remembers the choice too, so
/// signing in elsewhere switches that device over.
/// </summary>
public static class Languages
{
    public const string Default = "en";
    private const string StorageKey = "hearth-language";

    /// <summary>Each language with its name in that language, as people look for it.</summary>
    public static readonly IReadOnlyList<(string Code, string Name)> All = [("en", "English"), ("nl", "Nederlands")];

    public static string Current => CultureInfo.CurrentUICulture.Name is var name && IsSupported(name) ? name : Default;

    public static bool IsSupported(string? code) => All.Any(l => l.Code == code);

    /// <summary>The saved choice, or the browser's language when it's one the app has.</summary>
    public static async Task<string> ReadAsync(IJSRuntime js)
    {
        try
        {
            var saved = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (IsSupported(saved))
                return saved!;

            var browser = (await js.InvokeAsync<string>("hearth.browserLanguage")).Split('-')[0].ToLowerInvariant();
            return IsSupported(browser) ? browser : Default;
        }
        catch (JSException)
        {
            return Default;
        }
    }

    public static void Apply(string code)
    {
        var culture = new CultureInfo(code);
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
    }

    /// <summary>Remembers the choice on this device. It takes effect when the app is loaded again.</summary>
    public static async Task SaveAsync(IJSRuntime js, string code)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", StorageKey, code);
        }
        catch (JSException)
        {
            // Storage blocked: the account still has it.
        }
    }
}
