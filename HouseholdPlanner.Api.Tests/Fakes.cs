using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services;
using HouseholdPlanner.Api.Services.Interfaces;

namespace HouseholdPlanner.Api.Tests;

internal class FixedTime(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;
    public override DateTimeOffset GetUtcNow() => Now;
}

internal class FakeTokenService : ITokenService
{
    public string GenerateToken(User user) => $"token-for-{user.Username}";
}

internal class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public Task<EntityResult<User>> GetByIdAsync(Guid id) => Found(Users.FirstOrDefault(u => u.Id == id));

    public Task<EntityResult<User>> GetByUsernameAsync(string username)
        => Found(Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));

    public Task<List<User>> GetByHouseholdAsync(Guid householdId)
        => Task.FromResult(Users.Where(u => u.HouseholdId == householdId).ToList());

    public Task<bool> ExistsAsync(string username, Guid? exceptUserId = null)
        => Task.FromResult(Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Id != exceptUserId));

    public Task<EntityResult<User>> CreateAsync(User user)
    {
        Users.Add(user);
        return Task.FromResult(new EntityResult<User>(user, true, null));
    }

    public Task<Result> UpdateAsync(User user) => Task.FromResult(new Result(null, true));

    public Task DeleteAsync(User user)
    {
        Users.Remove(user);
        return Task.CompletedTask;
    }

    private static Task<EntityResult<User>> Found(User? user) => Task.FromResult(user is null
        ? new EntityResult<User>(null, false, "User not found.", ErrorCode.NotFound)
        : new EntityResult<User>(user, true, null));
}

internal class FakeHouseholdRepository : IHouseholdRepository
{
    public List<Household> Households { get; } = [];

    public Task<EntityResult<Household>> GetByIdAsync(Guid id) => Found(Households.FirstOrDefault(h => h.Id == id));

    public Task<EntityResult<Household>> GetByInviteCodeAsync(string inviteCode)
        => Found(Households.FirstOrDefault(h => h.InviteCode == InviteCodes.Normalize(inviteCode)));

    public Task SaveAsync() => Task.CompletedTask;

    private static Task<EntityResult<Household>> Found(Household? household) => Task.FromResult(household is null
        ? new EntityResult<Household>(null, false, "That invite code doesn't match a household.", ErrorCode.NotFound)
        : new EntityResult<Household>(household, true, null));
}

/// <summary>Keeps tasks and completions in lists. Like EF, changes to the objects count as saved on <see cref="SaveAsync"/>.</summary>
internal class FakeTaskRepository : ITaskRepository
{
    public List<HouseholdTask> Tasks { get; } = [];
    public List<TaskCompletion> Completions { get; } = [];
    public int Saves { get; private set; }

    public Task<List<HouseholdTask>> GetForHouseholdAsync(Guid householdId, DateTime doneSince)
        => Task.FromResult(Tasks.Where(t => t.HouseholdId == householdId && (!t.Done || t.DoneAt >= doneSince)).ToList());

    public Task<EntityResult<HouseholdTask>> GetByIdAsync(Guid householdId, Guid id)
    {
        var task = Tasks.FirstOrDefault(t => t.Id == id && t.HouseholdId == householdId);
        return Task.FromResult(task is null
            ? new EntityResult<HouseholdTask>(null, false, "Task not found.", ErrorCode.NotFound)
            : new EntityResult<HouseholdTask>(task, true, null));
    }

    public Task<HouseholdTask> CreateAsync(HouseholdTask task)
    {
        Tasks.Add(task);
        return Task.FromResult(task);
    }

    public Task SaveAsync()
    {
        Saves++;
        return Task.CompletedTask;
    }

    // Like the database: completions of a deleted task stay, without the task.
    public Task DeleteAsync(HouseholdTask task)
    {
        Tasks.Remove(task);
        foreach (var completion in Completions.Where(c => c.TaskId == task.Id))
            (completion.TaskId, completion.Task) = (null, null);
        return Task.CompletedTask;
    }

    public void AddCompletion(TaskCompletion completion)
    {
        completion.Task = Tasks.FirstOrDefault(t => t.Id == completion.TaskId);
        Completions.Add(completion);
    }

    public Task<EntityResult<TaskCompletion>> GetCompletionAsync(Guid householdId, Guid id)
    {
        var completion = Completions.FirstOrDefault(c => c.Id == id && c.HouseholdId == householdId);
        return Task.FromResult(completion is null
            ? new EntityResult<TaskCompletion>(null, false, "Completion not found.", ErrorCode.NotFound)
            : new EntityResult<TaskCompletion>(completion, true, null));
    }

    public Task<TaskCompletion?> GetLastCompletionAsync(Guid householdId, Guid taskId)
        => Task.FromResult(Completions.LastOrDefault(c => c.HouseholdId == householdId && c.TaskId == taskId));

    public void RemoveCompletion(TaskCompletion completion) => Completions.Remove(completion);

    public Task<Dictionary<Guid, int>> CountCompletionsByUserAsync(Guid householdId, DateOnly from)
        => Task.FromResult(Completions.Where(c => c.HouseholdId == householdId && c.On >= from)
            .GroupBy(c => c.UserId).ToDictionary(g => g.Key, g => g.Count()));

    public Task<Dictionary<Guid, int>> CountOpenByAssigneeAsync(Guid householdId)
        => Task.FromResult(Tasks.Where(t => t.HouseholdId == householdId && !t.Done && t.AssigneeId != null)
            .GroupBy(t => t.AssigneeId!.Value).ToDictionary(g => g.Key, g => g.Count()));
}

internal class FakeGroceryRepository : IGroceryRepository
{
    public List<GroceryItem> Items { get; } = [];

    public Task<List<GroceryItem>> GetListAsync(Guid householdId)
        => Task.FromResult(Items.Where(g => g.HouseholdId == householdId && g.ClearedAt == null).ToList());

    public Task<List<string>> GetRecentNamesAsync(Guid householdId, int take)
        => Task.FromResult(Items.Where(g => g.HouseholdId == householdId && g.ClearedAt != null)
            .OrderByDescending(g => g.ClearedAt).Select(g => g.Name)
            .DistinctBy(n => n.ToLowerInvariant()).Take(take).ToList());

    public Task<EntityResult<GroceryItem>> GetByIdAsync(Guid householdId, Guid id)
    {
        var item = Items.FirstOrDefault(g => g.Id == id && g.HouseholdId == householdId && g.ClearedAt == null);
        return Task.FromResult(item is null
            ? new EntityResult<GroceryItem>(null, false, "That item is no longer on the list.", ErrorCode.NotFound)
            : new EntityResult<GroceryItem>(item, true, null));
    }

    public Task<GroceryItem?> FindToBuyAsync(Guid householdId, string name)
        => Task.FromResult(Items.FirstOrDefault(g => g.HouseholdId == householdId && g.ClearedAt == null && !g.InCart
                                                     && g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));

    public Task<List<GroceryItem>> GetInCartAsync(Guid householdId)
        => Task.FromResult(Items.Where(g => g.HouseholdId == householdId && g.ClearedAt == null && g.InCart).ToList());

    public Task<List<GroceryItem>> GetClearedAsync(Guid householdId, IReadOnlyCollection<Guid> ids)
        => Task.FromResult(Items.Where(g => g.HouseholdId == householdId && g.ClearedAt != null && ids.Contains(g.Id)).ToList());

    public Task<GroceryItem> CreateAsync(GroceryItem item)
    {
        Items.Add(item);
        return Task.FromResult(item);
    }

    public Task SaveAsync() => Task.CompletedTask;

    public Task DeleteAsync(GroceryItem item)
    {
        Items.Remove(item);
        return Task.CompletedTask;
    }

    public Task PurgeClearedAsync(Guid householdId, DateTime before)
    {
        Items.RemoveAll(g => g.HouseholdId == householdId && g.ClearedAt < before);
        return Task.CompletedTask;
    }
}

internal class FakeApiKeyRepository : IApiKeyRepository
{
    public List<ApiKey> Keys { get; } = [];

    public Task<List<ApiKey>> GetForUserAsync(Guid userId)
        => Task.FromResult(Keys.Where(k => k.UserId == userId).OrderBy(k => k.CreatedAt).ToList());

    public Task<EntityResult<ApiKey>> GetByIdAsync(Guid userId, Guid id)
    {
        var key = Keys.FirstOrDefault(k => k.Id == id && k.UserId == userId);
        return Task.FromResult(key is null
            ? new EntityResult<ApiKey>(null, false, "That API key no longer exists.", ErrorCode.NotFound)
            : new EntityResult<ApiKey>(key, true, null));
    }

    public Task<EntityResult<ApiKey>> CreateAsync(ApiKey key)
    {
        Keys.Add(key);
        return Task.FromResult(new EntityResult<ApiKey>(key, true, null));
    }

    public Task DeleteAsync(ApiKey key)
    {
        Keys.Remove(key);
        return Task.CompletedTask;
    }
}
