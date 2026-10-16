using System.Globalization;
using HouseholdPlanner.Web.Models;
using Microsoft.Extensions.Localization;

namespace HouseholdPlanner.Web.Services;

/// <summary>
/// Due dates, repeats and dates in words, in the app's language. The app runs without .NET's culture data
/// (see the .csproj), so day and month names come from the translations too.
/// </summary>
public class DateText(IStringLocalizer<Strings> t)
{
    /// <summary>"2 days late", "Today", "Sat", "Oct 28" or "No date".</summary>
    public string Label(DateOnly? due, DateOnly today)
    {
        if (due is not { } date)
            return t["Due_NoDate"];
        return DueDates.DaysFrom(today, date) switch
        {
            < -1 and var n => t["Due_DaysLate", -n],
            -1             => t["Due_DayLate"],
            0              => t["Due_Today"],
            1              => t["Due_Tomorrow"],
            < 7            => Weekday(date),
            _              => Short(date)
        };
    }

    /// <summary>For "Next one …": "tomorrow", "on Thu" or "on Oct 22".</summary>
    public string Word(DateOnly due, DateOnly today) => DueDates.DaysFrom(today, due) switch
    {
        1   => t["Due_WordTomorrow"],
        < 7 => t["Due_WordOn", Weekday(due)],
        _   => t["Due_WordOn", Short(due)]
    };

    /// <summary>"Thu".</summary>
    public string Weekday(DateOnly date) => t[$"DayShort_{(int)date.DayOfWeek}"];

    /// <summary>"Oct 22".</summary>
    public string Short(DateOnly date) => t["Date_Short", t[$"MonthShort_{date.Month}"], date.Day];

    /// <summary>"Thursday".</summary>
    public string DayName(DayOfWeek day) => t[$"DayLong_{(int)day}"];

    /// <summary>"Thursday, October 15".</summary>
    public string Long(DateOnly date) => t["Date_Long", DayName(date.DayOfWeek), t[$"MonthLong_{date.Month}"], date.Day];

    public string Repeat(TaskRepeat repeat) => t[$"Repeat_{repeat}"];

    public string Group(DueGroup group) => t[$"Group_{group}"];

    public string Choice(DueChoice kind) => kind switch
    {
        DueChoice.Today       => t["Due_Today"],
        DueChoice.Tomorrow    => t["Due_Tomorrow"],
        DueChoice.ThisWeekend => t["Choice_ThisWeekend"],
        DueChoice.NextWeek    => t["Choice_NextWeek"],
        DueChoice.NoDate      => t["Due_NoDate"],
        _                     => t["Choice_PickDate"]
    };

    /// <summary>
    /// A culture for MudBlazor's calendar, with the translated month and day names. The app's own culture has no
    /// such names. The week starts on the household's day, which the calendar is given separately.
    /// </summary>
    public CultureInfo Calendar => _calendar ??= CreateCalendarCulture();

    private CultureInfo? _calendar;

    private CultureInfo CreateCalendarCulture()
    {
        var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        var format = culture.DateTimeFormat;
        string[] Months(string key) => [.. Enumerable.Range(1, 12).Select(m => t[$"{key}_{m}"].Value), ""];
        string[] Days(string key) => [.. Enumerable.Range(0, 7).Select(d => t[$"{key}_{d}"].Value)];

        format.MonthNames = format.MonthGenitiveNames = Months("MonthLong");
        format.AbbreviatedMonthNames = format.AbbreviatedMonthGenitiveNames = Months("MonthShort");
        format.DayNames = Days("DayLong");
        // MudBlazor heads the calendar's columns with these: one letter, like S M T W T F S.
        format.AbbreviatedDayNames = format.ShortestDayNames = [.. Days("DayShort").Select(d => d[..1].ToUpperInvariant())];
        // And titles it with this: "October 2026", "oktober 2026".
        format.YearMonthPattern = "MMMM yyyy";
        return culture;
    }
}
