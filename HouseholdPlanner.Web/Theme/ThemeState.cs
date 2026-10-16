using Microsoft.JSInterop;

namespace HouseholdPlanner.Web.Theme;

public enum ThemePreference
{
    System,
    Light,
    Dark
}

/// <summary>Light or dark: follows the system unless the user picked one, which is remembered on this device.</summary>
public class ThemeState(IJSRuntime js)
{
    private const string Key = "hearth-theme";

    public ThemePreference Preference { get; private set; } = ThemePreference.System;
    public bool SystemIsDark { get; private set; }
    public bool IsDark => Preference == ThemePreference.Dark || (Preference == ThemePreference.System && SystemIsDark);

    public event Action? Changed;

    public async Task LoadAsync(bool systemIsDark)
    {
        SystemIsDark = systemIsDark;
        try
        {
            if (Enum.TryParse<ThemePreference>(await js.InvokeAsync<string?>("localStorage.getItem", Key), out var saved))
                Preference = saved;
        }
        catch (JSException)
        {
            // Storage blocked (private browsing): follow the system.
        }
        Changed?.Invoke();
    }

    public void SetSystemIsDark(bool dark)
    {
        SystemIsDark = dark;
        Changed?.Invoke();
    }

    public async Task SetPreferenceAsync(ThemePreference preference)
    {
        Preference = preference;
        Changed?.Invoke();
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", Key, preference.ToString());
        }
        catch (JSException)
        {
            // Only lasts until the page is reloaded then.
        }
    }

    /// <summary>A member's avatar colour, from their hue, in a lightness that fits the theme.</summary>
    public string AvatarColor(int hue) => $"oklch({(IsDark ? 0.5 : 0.84)} 0.075 {hue})";

    /// <summary>The text colour on an avatar.</summary>
    public string AvatarInk => IsDark ? "#fbf7f2" : "#2a2420";

    /// <summary>A stronger version of a member's colour, for their bar in the stats.</summary>
    public static string BarColor(int hue) => $"oklch(0.66 0.11 {hue})";
}
