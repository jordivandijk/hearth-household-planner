using System.Text.Json.Serialization;

namespace HouseholdPlanner.Web.Models;

/// <summary>In the order the task sheet shows them. Sent to the API by name.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TaskRepeat>))]
public enum TaskRepeat
{
    None,
    Daily,
    Weekly,
    /// <summary>Every two weeks.</summary>
    Biweekly,
    Monthly
}

public class HouseholdTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }
    public DateOnly? Due { get; set; }
    public TaskRepeat Repeat { get; set; }
    public bool Done { get; set; }
    public Guid? DoneById { get; set; }
    public DateTime? DoneAt { get; set; }

    public SaveTaskRequest ToRequest() => new() { Title = Title, AssigneeId = AssigneeId, Due = Due, Repeat = Repeat };
}

public class SaveTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }
    public DateOnly? Due { get; set; }
    public TaskRepeat Repeat { get; set; }
}

public class CompleteTaskRequest
{
    public DateOnly Today { get; set; }
}

public class CompleteTaskResponse
{
    public HouseholdTask Task { get; set; } = new();
    public Guid CompletionId { get; set; }
}
