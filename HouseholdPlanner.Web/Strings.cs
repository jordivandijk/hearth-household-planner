using System.Resources;

namespace HouseholdPlanner.Web;

/// <summary>
/// Marker for the app's texts in Resources/Strings.resx (English) and Strings.nl.resx (Dutch). Components get
/// them through <c>IStringLocalizer&lt;Strings&gt;</c>.
/// </summary>
public class Strings
{
    private static readonly ResourceManager Resources = new("HouseholdPlanner.Web.Resources.Strings", typeof(Strings).Assembly);

    /// <summary>A text in the current language, for code outside components, like the HTTP helpers.</summary>
    public static string Text(string key) => Resources.GetString(key) ?? key;
}
