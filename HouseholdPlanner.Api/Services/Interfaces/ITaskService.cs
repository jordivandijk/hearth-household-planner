using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Services.Interfaces;

public interface ITaskService
{
    /// <summary>Open tasks and the ones done in the last 30 days.</summary>
    Task<ValueResult<List<TaskResponse>>> GetAllAsync(Guid householdId);
    Task<ValueResult<TaskResponse>> CreateAsync(Guid householdId, SaveTaskRequest request);
    Task<ValueResult<TaskResponse>> UpdateAsync(Guid householdId, Guid id, SaveTaskRequest request);
    Task<Result> DeleteAsync(Guid householdId, Guid id);

    /// <summary>Marks the task done, or moves a repeating task to its next due date, and logs who did it.</summary>
    Task<ValueResult<CompleteTaskResponse>> CompleteAsync(Guid householdId, Guid userId, Guid id, CompleteTaskRequest request);
    /// <summary>Takes a completion back: the task is open again with its old due date.</summary>
    Task<ValueResult<TaskResponse?>> UndoCompletionAsync(Guid householdId, Guid completionId);
    /// <summary>Opens a done task again, and takes back the completion that made it done.</summary>
    Task<ValueResult<TaskResponse>> ReopenAsync(Guid householdId, Guid id);
}
