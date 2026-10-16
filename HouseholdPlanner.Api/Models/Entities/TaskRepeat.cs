namespace HouseholdPlanner.Api.Models.Entities;

/// <summary>How often a task comes back. Stored as its number, so new values go at the end.</summary>
public enum TaskRepeat
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    /// <summary>Every two weeks.</summary>
    Biweekly = 4
}
