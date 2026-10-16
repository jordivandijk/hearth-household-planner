using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Repositories.Interfaces;

public interface ITaskRepository
{
    /// <summary>The household's open tasks, and the ones done since <paramref name="doneSince"/>.</summary>
    Task<List<HouseholdTask>> GetForHouseholdAsync(Guid householdId, DateTime doneSince);
    Task<EntityResult<HouseholdTask>> GetByIdAsync(Guid householdId, Guid id);
    Task<HouseholdTask> CreateAsync(HouseholdTask task);
    /// <summary>Saves changes to tracked tasks and completions.</summary>
    Task SaveAsync();
    Task DeleteAsync(HouseholdTask task);

    void AddCompletion(TaskCompletion completion);
    Task<EntityResult<TaskCompletion>> GetCompletionAsync(Guid householdId, Guid id);
    /// <summary>The task's most recent completion, if it has one.</summary>
    Task<TaskCompletion?> GetLastCompletionAsync(Guid householdId, Guid taskId);
    void RemoveCompletion(TaskCompletion completion);
    /// <summary>How many tasks each member did from <paramref name="from"/> on.</summary>
    Task<Dictionary<Guid, int>> CountCompletionsByUserAsync(Guid householdId, DateOnly from);
    /// <summary>How many open tasks are assigned to each member.</summary>
    Task<Dictionary<Guid, int>> CountOpenByAssigneeAsync(Guid householdId);
}
