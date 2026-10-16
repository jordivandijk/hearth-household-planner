using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HouseholdPlanner.Api.Tests;

/// <summary>Every error the API can send has a Dutch version, so a new message can't slip through untranslated.</summary>
public partial class ErrorMessageTranslationTests
{
    private static readonly string Root = FindRoot();

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "HouseholdPlanner.slnx")))
            dir = dir.Parent;
        return dir!.FullName;
    }

    /// <summary>The messages of failed results in the API's code: <c>new("…", false, …)</c>, <c>new(null, false, "…", …)</c> and message constants.</summary>
    private static IEnumerable<string> ErrorMessagesInCode()
        => Directory.EnumerateFiles(Path.Combine(Root, "HouseholdPlanner.Api"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains("Migrations"))
            .SelectMany(f => FailedResult().Matches(File.ReadAllText(f)))
            .Select(m => m.Groups["message"].Value)
            .Distinct();

    private static Dictionary<string, string> Dutch()
        => XDocument.Load(Path.Combine(Root, "HouseholdPlanner.Api", "Resources", "ErrorMessages.nl.resx")).Root!
            .Elements("data").ToDictionary(d => d.Attribute("name")!.Value, d => d.Element("value")!.Value);

    [Fact]
    public void The_scan_finds_the_errors()
    {
        var found = ErrorMessagesInCode().ToList();
        Assert.Contains("Invalid username or password.", found);   // a service's own error
        Assert.Contains("Task not found.", found);                 // a repository's
        Assert.DoesNotContain("Task created.", found);             // successes aren't shown
    }

    [Fact]
    public void Every_error_message_has_a_Dutch_translation()
    {
        var dutch = Dutch();
        Assert.All(ErrorMessagesInCode(), message => Assert.True(dutch.ContainsKey(message), $"No Dutch for: {message}"));
    }

    [Theory]
    [InlineData("nl", "Onjuiste gebruikersnaam of wachtwoord.")]
    [InlineData("en", "Invalid username or password.")]
    public void Errors_come_back_in_the_requested_language(string language, string expected)
    {
        var factory = new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }), NullLoggerFactory.Instance);
        var localizer = new StringLocalizer<ErrorMessages>(factory);
        CultureInfo.CurrentUICulture = new CultureInfo(language);

        Assert.Equal(expected, localizer["Invalid username or password."].Value);
    }

    [GeneratedRegex("""new\("(?<message>[^"]+)", false|new\(null, false, "(?<message>[^"]+)"|Message = "(?<message>[^"]+)"|context\.Fail\("(?<message>[^"]+)"\)""")]
    private static partial Regex FailedResult();
}
