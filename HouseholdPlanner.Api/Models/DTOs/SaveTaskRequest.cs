using System.ComponentModel.DataAnnotations;
using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Models.DTOs;

/// <summary>Creates or edits a task.</summary>
public class SaveTaskRequest
{
    [Required, StringLength(200, MinimumLength = 1)] public string Title { get; set; } = string.Empty;
    /// <summary>Null means anyone.</summary>
    public Guid? AssigneeId { get; set; }
    public DateOnly? Due { get; set; }
    [EnumDataType(typeof(TaskRepeat))] public TaskRepeat Repeat { get; set; }
}
