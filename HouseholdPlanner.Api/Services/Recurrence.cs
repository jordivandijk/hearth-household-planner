using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Services;

public static class Recurrence
{
    /// <summary>
    /// When a repeating task is due again after it was done today. Counted from today rather than the
    /// old due date, so a chore done late doesn't come back overdue.
    /// </summary>
    public static DateOnly? NextDue(TaskRepeat repeat, DateOnly today) => repeat switch
    {
        TaskRepeat.Daily    => today.AddDays(1),
        TaskRepeat.Weekly   => today.AddDays(7),
        TaskRepeat.Biweekly => today.AddDays(14),
        TaskRepeat.Monthly  => today.AddMonths(1),
        _                   => null
    };
}
