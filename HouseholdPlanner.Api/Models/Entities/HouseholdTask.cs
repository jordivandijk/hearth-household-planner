namespace HouseholdPlanner.Api.Models.Entities;

/// <summary>A chore. A repeating task never gets done: completing it moves its due date on.</summary>
public class HouseholdTask : IEntity
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Null means anyone can do it.</summary>
    public Guid? AssigneeId { get; set; }
    public DateOnly? Due { get; set; }
    public TaskRepeat Repeat { get; set; }
    public bool Done { get; set; }
    public Guid? DoneById { get; set; }
    public DateTime? DoneAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Household Household { get; set; } = null!;
    public User? Assignee { get; set; }
    public User? DoneBy { get; set; }
}
