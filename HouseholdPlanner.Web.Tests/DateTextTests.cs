using HouseholdPlanner.Web.Models;
using HouseholdPlanner.Web.Services;

namespace HouseholdPlanner.Web.Tests;

public class DateTextTests
{
    /// <summary>A Thursday.</summary>
    private static readonly DateOnly Today = new(2026, 10, 15);

    private static DateText In(string language) => new(Translations.In(language));

    private static DateOnly? Day(int? offset) => offset is { } d ? Today.AddDays(d) : null;

    [Theory]
    [InlineData("en", null, "No date")]
    [InlineData("en", -3, "3 days late")]
    [InlineData("en", -1, "1 day late")]
    [InlineData("en", 0, "Today")]
    [InlineData("en", 1, "Tomorrow")]
    [InlineData("en", 2, "Sat")]
    [InlineData("en", 7, "Oct 22")]
    [InlineData("nl", null, "Geen datum")]
    [InlineData("nl", -3, "3 dagen te laat")]
    [InlineData("nl", -1, "1 dag te laat")]
    [InlineData("nl", 0, "Vandaag")]
    [InlineData("nl", 1, "Morgen")]
    [InlineData("nl", 2, "za")]
    [InlineData("nl", 7, "22 okt")]
    public void Due_dates_read_naturally(string language, int? days, string label)
        => Assert.Equal(label, In(language).Label(Day(days), Today));

    [Theory]
    [InlineData("en", 1, "tomorrow")]
    [InlineData("en", 3, "on Sun")]
    [InlineData("en", 31, "on Nov 15")]
    [InlineData("nl", 1, "morgen")]
    [InlineData("nl", 3, "op zo")]
    [InlineData("nl", 31, "op 15 nov")]
    public void Next_one_reads_as_a_word(string language, int days, string word)
        => Assert.Equal(word, In(language).Word(Today.AddDays(days), Today));

    [Theory]
    [InlineData("en", "Thursday, October 15")]
    [InlineData("nl", "donderdag 15 oktober")]
    public void The_long_date_is_written_the_way_each_language_does(string language, string date)
        => Assert.Equal(date, In(language).Long(Today));

    [Theory]
    [InlineData("en", TaskRepeat.Weekly, "Weekly")]
    [InlineData("nl", TaskRepeat.Weekly, "Wekelijks")]
    [InlineData("nl", TaskRepeat.None, "Nooit")]
    [InlineData("en", TaskRepeat.Biweekly, "Every 2 weeks")]
    [InlineData("nl", TaskRepeat.Biweekly, "Om de week")]
    public void Repeats_are_translated(string language, TaskRepeat repeat, string label)
        => Assert.Equal(label, In(language).Repeat(repeat));

    [Theory]
    [InlineData("en", DueGroup.ThisWeek, "This week")]
    [InlineData("nl", DueGroup.ThisWeek, "Deze week")]
    [InlineData("en", DueGroup.NextWeek, "Next week")]
    [InlineData("nl", DueGroup.NextWeek, "Volgende week")]
    [InlineData("nl", DueGroup.Someday, "Ooit")]
    public void Groups_are_translated(string language, DueGroup group, string title)
        => Assert.Equal(title, In(language).Group(group));

    [Theory]
    [InlineData("en", DueChoice.ThisWeekend, "This weekend")]
    [InlineData("nl", DueChoice.ThisWeekend, "Dit weekend")]
    [InlineData("nl", DueChoice.NoDate, "Geen datum")]
    [InlineData("en", DueChoice.PickDate, "Choose date")]
    [InlineData("nl", DueChoice.PickDate, "Kies datum")]
    public void When_choices_are_translated(string language, DueChoice choice, string label)
        => Assert.Equal(label, In(language).Choice(choice));

    [Theory]
    [InlineData("en", "October", "S")]
    [InlineData("nl", "oktober", "Z")]
    public void The_calendar_gets_translated_names(string language, string october, string sunday)
    {
        var calendar = In(language).Calendar.DateTimeFormat;

        Assert.Equal(october, calendar.MonthNames[9]);
        Assert.Equal(13, calendar.MonthNames.Length);
        Assert.Equal(sunday, calendar.AbbreviatedDayNames[0]);
        Assert.Equal(sunday, calendar.ShortestDayNames[0]);
        Assert.Equal(october + " 2026", new DateTime(2026, 10, 15).ToString("Y", In(language).Calendar));
    }

    [Theory]
    [InlineData("en", 1, "1 person")]
    [InlineData("en", 3, "3 people")]
    [InlineData("nl", 1, "1 persoon")]
    [InlineData("nl", 3, "3 personen")]
    [InlineData("nl", 0, "0 personen")]
    public void Counts_use_the_right_plural(string language, int count, string text)
        => Assert.Equal(text, Translations.In(language).Plural("Household_People", count));
}
