using System.Text.RegularExpressions;
using System.Xml.Linq;
using HouseholdPlanner.Web.Models;
using HouseholdPlanner.Web.Services;

namespace HouseholdPlanner.Web.Tests;

/// <summary>Keeps the English and Dutch texts in step, so a new text can't be missing a translation.</summary>
public partial class TranslationFileTests
{
    private static Dictionary<string, string> Read(string file)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "HouseholdPlanner.slnx")))
            dir = dir.Parent;
        var path = Path.Combine(dir!.FullName, "HouseholdPlanner.Web", "Resources", file);
        return XDocument.Load(path).Root!.Elements("data").ToDictionary(d => d.Attribute("name")!.Value, d => d.Element("value")!.Value);
    }

    private static readonly Dictionary<string, string> English = Read("Strings.resx");
    private static readonly Dictionary<string, string> Dutch = Read("Strings.nl.resx");

    [Fact]
    public void Both_languages_have_the_same_texts()
    {
        Assert.Empty(English.Keys.Except(Dutch.Keys));
        Assert.Empty(Dutch.Keys.Except(English.Keys));
    }

    [Fact]
    public void No_text_is_empty()
        => Assert.All(English.Concat(Dutch), t => Assert.False(string.IsNullOrWhiteSpace(t.Value), t.Key));

    [Fact]
    public void Both_languages_fill_in_the_same_placeholders()
        => Assert.All(English, t => Assert.Equal(Placeholders(t.Value), Placeholders(Dutch[t.Key])));

    [Fact]
    public void Every_value_the_app_looks_up_by_name_has_a_text()
    {
        var keys = Enum.GetValues<GroceryCategory>().Select(c => $"Aisle_{c}")
            .Concat(Enum.GetValues<DueGroup>().Select(g => $"Group_{g}"))
            .Concat(Enum.GetValues<TaskRepeat>().Select(r => $"Repeat_{r}"))
            .Concat(Enumerable.Range(0, 7).SelectMany(d => new[] { $"DayShort_{d}", $"DayLong_{d}" }))
            .Concat(Enumerable.Range(1, 12).SelectMany(m => new[] { $"MonthShort_{m}", $"MonthLong_{m}" }));

        Assert.All(keys, key => Assert.Contains(key, English.Keys));
    }

    private static string[] Placeholders(string text) => Placeholder().Matches(text).Select(m => m.Value).Distinct().Order().ToArray();

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex Placeholder();
}
