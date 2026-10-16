using HouseholdPlanner.Web.Models;
using HouseholdPlanner.Web.Services;

namespace HouseholdPlanner.Web.Tests;

public class DueDatesTests
{
    /// <summary>A Thursday.</summary>
    private static readonly DateOnly Today = new(2026, 10, 15);

    [Theory]
    [InlineData(null, DueGroup.Someday)]
    [InlineData(-10, DueGroup.Overdue)]
    [InlineData(-1, DueGroup.Overdue)]
    [InlineData(0, DueGroup.Today)]
    [InlineData(1, DueGroup.Tomorrow)]
    [InlineData(2, DueGroup.ThisWeek)]    // Saturday
    [InlineData(3, DueGroup.ThisWeek)]    // Sunday, the last day of this week
    [InlineData(4, DueGroup.NextWeek)]    // Monday
    [InlineData(10, DueGroup.NextWeek)]   // the Sunday after
    [InlineData(11, DueGroup.Later)]      // the Monday after that
    public void Tasks_fall_in_the_right_group_with_weeks_from_monday(int? days, DueGroup group)
        => Assert.Equal(group, DueDates.Group(days is { } d ? Today.AddDays(d) : null, Today, DayOfWeek.Monday));

    [Theory]
    [InlineData(2, DueGroup.ThisWeek)]    // Saturday, the last day of this week
    [InlineData(3, DueGroup.NextWeek)]    // Sunday
    [InlineData(9, DueGroup.NextWeek)]    // the Saturday after
    [InlineData(10, DueGroup.Later)]      // the Sunday after that
    public void Tasks_fall_in_the_right_group_with_weeks_from_sunday(int days, DueGroup group)
        => Assert.Equal(group, DueDates.Group(Today.AddDays(days), Today, DayOfWeek.Sunday));

    [Theory]
    [InlineData(1, DueGroup.Tomorrow)]    // Monday: tomorrow wins over next week
    [InlineData(2, DueGroup.NextWeek)]
    [InlineData(7, DueGroup.NextWeek)]    // next Sunday
    [InlineData(8, DueGroup.Later)]
    public void On_the_last_day_of_the_week_the_rest_is_next_week(int days, DueGroup group)
    {
        var sunday = new DateOnly(2026, 10, 18);
        Assert.Equal(group, DueDates.Group(sunday.AddDays(days), sunday, DayOfWeek.Monday));
    }

    [Theory]
    [InlineData(2026, 10, 15, DayOfWeek.Monday, 12)]    // Thursday
    [InlineData(2026, 10, 12, DayOfWeek.Monday, 12)]    // the Monday itself
    [InlineData(2026, 10, 18, DayOfWeek.Monday, 12)]    // Sunday
    [InlineData(2026, 10, 15, DayOfWeek.Sunday, 11)]
    [InlineData(2026, 10, 15, DayOfWeek.Saturday, 10)]
    public void A_week_starts_on_the_chosen_day(int year, int month, int day, DayOfWeek firstDay, int startDay)
        => Assert.Equal(new DateOnly(2026, 10, startDay), DueDates.WeekStart(new DateOnly(year, month, day), firstDay));

    [Fact]
    public void Groups_are_listed_in_order()
        => Assert.Equal([DueGroup.Overdue, DueGroup.Today, DueGroup.Tomorrow, DueGroup.ThisWeek, DueGroup.NextWeek, DueGroup.Later, DueGroup.Someday], DueDates.Groups);

    [Fact]
    public void When_choices_include_this_weekend_when_saturday_is_two_or_more_days_away()
    {
        var choices = DueDates.Choices(Today);

        Assert.Equal([DueChoice.Today, DueChoice.Tomorrow, DueChoice.ThisWeekend, DueChoice.NextWeek, DueChoice.NoDate, DueChoice.PickDate], choices.Select(c => c.Kind));
        Assert.Equal(new DateOnly(2026, 10, 17), choices.Single(c => c.Kind == DueChoice.ThisWeekend).Date);
        Assert.Equal(Today.AddDays(7), choices.Single(c => c.Kind == DueChoice.NextWeek).Date);
        Assert.Null(choices.Single(c => c.Kind == DueChoice.NoDate).Date);
    }

    [Theory]
    [InlineData(2026, 10, 16)]   // Friday: Saturday is tomorrow
    [InlineData(2026, 10, 17)]   // Saturday itself
    public void When_choices_skip_this_weekend_when_it_is_that_close(int year, int month, int day)
        => Assert.DoesNotContain(DueChoice.ThisWeekend, DueDates.Choices(new DateOnly(year, month, day)).Select(c => c.Kind));

    [Theory]
    [InlineData(0, DueChoice.Today)]
    [InlineData(1, DueChoice.Tomorrow)]
    [InlineData(2, DueChoice.ThisWeekend)]   // Saturday
    [InlineData(7, DueChoice.NextWeek)]
    [InlineData(null, DueChoice.NoDate)]
    [InlineData(20, DueChoice.PickDate)]     // matches no preset: opens the calendar on it
    [InlineData(-3, DueChoice.PickDate)]     // an overdue task too
    public void A_tasks_due_date_shows_as_its_preset_or_as_choose_date(int? days, DueChoice choice)
        => Assert.Equal(choice, DueDates.ChoiceFor(days is { } d ? Today.AddDays(d) : null, Today));

    [Fact]
    public void Choose_date_has_no_date_of_its_own()
        => Assert.Null(DueDates.Choices(Today).Single(c => c.Kind == DueChoice.PickDate).Date);
}
