namespace HouseholdPlanner.Api.Models.Entities;

/// <summary>The people who share one task list and one grocery list. New members join with the invite code.</summary>
public class Household : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    /// <summary>The first day of a calendar week, for "This week" and "Next week" in the task list and the calendar.</summary>
    public DayOfWeek WeekStartsOn { get; set; } = DayOfWeek.Monday;
    public DateTime CreatedAt { get; set; }

    public ICollection<User> Members { get; set; } = [];
}
