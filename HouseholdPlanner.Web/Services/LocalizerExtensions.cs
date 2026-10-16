using Microsoft.Extensions.Localization;

namespace HouseholdPlanner.Web.Services;

public static class LocalizerExtensions
{
    /// <summary>
    /// The text for a count: the key with "_One" for 1 and "_Other" otherwise, e.g. "1 task" and "3 tasks".
    /// Enough for English and Dutch; a language with more plural forms would need more keys.
    /// </summary>
    public static string Plural(this IStringLocalizer t, string key, int count)
        => t[count == 1 ? $"{key}_One" : $"{key}_Other", count];
}
