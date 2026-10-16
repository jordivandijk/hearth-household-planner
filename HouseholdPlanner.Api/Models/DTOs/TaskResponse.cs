using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Models.DTOs;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }
    public DateOnly? Due { get; set; }
    public TaskRepeat Repeat { get; set; }
    public bool Done { get; set; }
    public Guid? DoneById { get; set; }
    public DateTime? DoneAt { get; set; }
}
