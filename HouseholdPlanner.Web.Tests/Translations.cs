using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HouseholdPlanner.Web.Tests;

/// <summary>The app's real translations, set up the way Program.cs does, for one language.</summary>
internal static class Translations
{
    private static readonly ResourceManagerStringLocalizerFactory Factory =
        new(Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }), NullLoggerFactory.Instance);

    /// <summary>Switches this test to <paramref name="language"/> and returns the app's texts.</summary>
    public static IStringLocalizer<Strings> In(string language)
    {
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(language);
        return new StringLocalizer<Strings>(Factory);
    }
}
