using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.Services;

/// <summary>The group a task falls in.</summary>
public enum DueGroup
{
    Overdue,
    Today,
    Tomorrow,
    ThisWeek,
    NextWeek,
    Later,
    Someday
}

/// <summary>The "When" choices in the task sheet. <see cref="PickDate"/> opens a calendar for any other day.</summary>
public enum DueChoice
{
    Today,
    Tomorrow,
    ThisWeekend,
    NextWeek,
    NoDate,
    PickDate
}

/// <summary>Date maths for due dates. How they read is in <see cref="DateText"/>.</summary>
public static class DueDates
{
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    public static int DaysFrom(DateOnly today, DateOnly due) => due.DayNumber - today.DayNumber;

    /// <summary>The first day of the calendar week that <paramref name="date"/> is in.</summary>
    public static DateOnly WeekStart(DateOnly date, DayOfWeek firstDay)
        => date.AddDays(-(((int)date.DayOfWeek - (int)firstDay + 7) % 7));

    /// <summary>
    /// The group of a task due on <paramref name="due"/>. This week and Next week are calendar weeks starting on
    /// <paramref name="firstDay"/>; today and tomorrow have their own groups, even when tomorrow is in next week.
    /// </summary>
    public static DueGroup Group(DateOnly? due, DateOnly today, DayOfWeek firstDay)
    {
        if (due is not { } date)
            return DueGroup.Someday;

        var nextWeek = WeekStart(today, firstDay).AddDays(7);
        return DaysFrom(today, date) switch
        {
            < 0                               => DueGroup.Overdue,
            0                                 => DueGroup.Today,
            1                                 => DueGroup.Tomorrow,
            _ when date < nextWeek            => DueGroup.ThisWeek,
            _ when date < nextWeek.AddDays(7) => DueGroup.NextWeek,
            _                                 => DueGroup.Later
        };
    }

    /// <summary>The groups in the order the list shows them.</summary>
    public static IReadOnlyList<DueGroup> Groups { get; } = Enum.GetValues<DueGroup>();

    /// <summary>The "When" choices with their dates. Choose date has none: the calendar gives it one.</summary>
    public static List<(DueChoice Kind, DateOnly? Date)> Choices(DateOnly today)
    {
        var choices = new List<(DueChoice, DateOnly?)> { (DueChoice.Today, today), (DueChoice.Tomorrow, today.AddDays(1)) };

        var toSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        if (toSaturday >= 2)
            choices.Add((DueChoice.ThisWeekend, today.AddDays(toSaturday)));

        choices.Add((DueChoice.NextWeek, today.AddDays(7)));
        choices.Add((DueChoice.NoDate, null));
        choices.Add((DueChoice.PickDate, null));
        return choices;
    }

    /// <summary>The choice that shows a task's due date: a preset when one matches, otherwise Choose date.</summary>
    public static DueChoice ChoiceFor(DateOnly? due, DateOnly today)
        => Choices(today).Where(c => c.Kind != DueChoice.PickDate && c.Date == due).Select(c => c.Kind).DefaultIfEmpty(DueChoice.PickDate).First();
}
