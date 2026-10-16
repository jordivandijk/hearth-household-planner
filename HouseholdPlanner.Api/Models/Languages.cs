namespace HouseholdPlanner.Api.Models;

/// <summary>The languages the app is translated into, as two-letter codes.</summary>
public static class Languages
{
    public const string Default = "en";

    public static readonly IReadOnlyList<string> Supported = ["en", "nl"];

    public static bool IsSupported(string? language) => language is not null && Supported.Contains(language);
}
