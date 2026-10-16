namespace HouseholdPlanner.Api.Models.DTOs;

public class CompleteTaskRequest
{
    /// <summary>Today in the user's own time zone, which the server doesn't know.</summary>
    public DateOnly Today { get; set; }
}
