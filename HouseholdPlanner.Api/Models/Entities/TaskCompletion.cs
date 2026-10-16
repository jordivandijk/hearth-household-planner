namespace HouseholdPlanner.Api.Models.Entities;

/// <summary>
/// One time someone did a task, for the household's weekly stats. It remembers the task's due date
/// from before, so the completion can be undone.
/// </summary>
public class TaskCompletion : IEntity
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>The day it was done, by the clock of the device it was done on.</summary>
    public DateOnly On { get; set; }
    public DateOnly? PreviousDue { get; set; }
    public DateTime CreatedAt { get; set; }

    public HouseholdTask? Task { get; set; }
    public User User { get; set; } = null!;
}
