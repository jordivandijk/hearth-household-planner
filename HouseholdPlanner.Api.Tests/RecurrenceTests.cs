using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Services;

namespace HouseholdPlanner.Api.Tests;

public class RecurrenceTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);

    [Theory]
    [InlineData(TaskRepeat.Daily, 2026, 10, 16)]
    [InlineData(TaskRepeat.Weekly, 2026, 10, 22)]
    [InlineData(TaskRepeat.Biweekly, 2026, 10, 29)]
    [InlineData(TaskRepeat.Monthly, 2026, 11, 15)]
    public void Next_due_counts_from_today(TaskRepeat repeat, int year, int month, int day)
        => Assert.Equal(new DateOnly(year, month, day), Recurrence.NextDue(repeat, Today));

    [Fact]
    public void Monthly_from_the_31st_ends_on_the_last_day_of_a_shorter_month()
        => Assert.Equal(new DateOnly(2026, 2, 28), Recurrence.NextDue(TaskRepeat.Monthly, new DateOnly(2026, 1, 31)));

    [Fact]
    public void Not_repeating_has_no_next_due_date()
        => Assert.Null(Recurrence.NextDue(TaskRepeat.None, Today));
}
