using HouseholdPlanner.Api.Data;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Api.Repositories.Implementations;

public class TaskRepository(AppDbContext context) : ITaskRepository
{
    public Task<List<HouseholdTask>> GetForHouseholdAsync(Guid householdId, DateTime doneSince)
        => context.Tasks
            .Where(t => t.HouseholdId == householdId && (!t.Done || t.DoneAt >= doneSince))
            .OrderBy(t => t.Due == null).ThenBy(t => t.Due).ThenBy(t => t.CreatedAt)
            .ToListAsync();

    public async Task<EntityResult<HouseholdTask>> GetByIdAsync(Guid householdId, Guid id)
    {
        var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId);
        return task is null
            ? new(null, false, "Task not found.", ErrorCode.NotFound)
            : new(task, true, "Task found.");
    }

    public async Task<HouseholdTask> CreateAsync(HouseholdTask task)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        return task;
    }

    public Task SaveAsync() => context.SaveChangesAsync();

    public async Task DeleteAsync(HouseholdTask task)
    {
        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
    }

    public void AddCompletion(TaskCompletion completion) => context.TaskCompletions.Add(completion);

    public async Task<EntityResult<TaskCompletion>> GetCompletionAsync(Guid householdId, Guid id)
    {
        var completion = await context.TaskCompletions
            .Include(c => c.Task)
            .FirstOrDefaultAsync(c => c.Id == id && c.HouseholdId == householdId);
        return completion is null
            ? new(null, false, "Completion not found.", ErrorCode.NotFound)
            : new(completion, true, "Completion found.");
    }

    public Task<TaskCompletion?> GetLastCompletionAsync(Guid householdId, Guid taskId)
        => context.TaskCompletions
            .Where(c => c.HouseholdId == householdId && c.TaskId == taskId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

    public void RemoveCompletion(TaskCompletion completion) => context.TaskCompletions.Remove(completion);

    public Task<Dictionary<Guid, int>> CountCompletionsByUserAsync(Guid householdId, DateOnly from)
        => context.TaskCompletions
            .Where(c => c.HouseholdId == householdId && c.On >= from)
            .GroupBy(c => c.UserId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

    public Task<Dictionary<Guid, int>> CountOpenByAssigneeAsync(Guid householdId)
        => context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.Done && t.AssigneeId != null)
            .GroupBy(t => t.AssigneeId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
}
