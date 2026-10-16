using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Interfaces;

public interface ITaskApiClient
{
    Task<List<HouseholdTask>> GetAllAsync();
    Task<HouseholdTask> CreateAsync(SaveTaskRequest request);
    Task<HouseholdTask> UpdateAsync(Guid id, SaveTaskRequest request);
    Task DeleteAsync(Guid id);
    Task<CompleteTaskResponse> CompleteAsync(Guid id, DateOnly today);
    Task<HouseholdTask> ReopenAsync(Guid id);
    /// <summary>Returns the task as it is again, or null when it was deleted meanwhile.</summary>
    Task<HouseholdTask?> UndoCompletionAsync(Guid completionId);
}
