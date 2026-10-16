namespace HouseholdPlanner.Api.Models.DTOs;

public class CompleteTaskResponse
{
    public TaskResponse Task { get; set; } = new();
    /// <summary>Undoes the completion when deleted.</summary>
    public Guid CompletionId { get; set; }
}
