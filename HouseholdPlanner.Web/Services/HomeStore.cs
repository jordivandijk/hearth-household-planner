using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.Services;

/// <summary>
/// The household's tasks and groceries, shared by the pages and the nav badges. Every change goes to the
/// API first and then updates the lists here, so the pages only re-render.
/// </summary>
public class HomeStore(IAuthApiClient authApi, IHouseholdApiClient householdApi, ITaskApiClient taskApi, IGroceryApiClient groceryApi)
{
    public UserProfile? Me { get; private set; }
    public Household? Household { get; private set; }
    public List<HouseholdTask> Tasks { get; private set; } = [];
    public List<GroceryItem> Groceries { get; private set; } = [];
    public List<string> Recents { get; private set; } = [];
    public bool Loaded { get; private set; }

    public event Action? Changed;

    /// <summary>
    /// Counts the changes made here. A refresh that was on its way during a change may have been read before it,
    /// and would undo it on screen, e.g. a ticked grocery jumping back on a slow network. So it reads again.
    /// </summary>
    private int _changes;

    public IReadOnlyList<Member> Members => Household?.Members ?? [];

    /// <summary>The first day of the household's calendar weeks.</summary>
    public DayOfWeek WeekStart => Household?.WeekStartsOn ?? DayOfWeek.Monday;

    public Member? MemberById(Guid? id) => id is null ? null : Members.FirstOrDefault(m => m.Id == id);

    /// <summary>For the nav badge: open tasks due today or overdue, for me or for anyone.</summary>
    public int TasksDueNow => Tasks.Count(t => !t.Done && t.Due <= DueDates.Today && (t.AssigneeId == Me?.Id || t.AssigneeId is null));

    public int GroceriesToBuy => Groceries.Count(g => !g.InCart);

    public async Task LoadAsync()
    {
        var me = authApi.GetMeAsync();
        await Task.WhenAll(me, RefreshAsync(notify: false));
        Me = me.Result;
        Loaded = true;
        Notify();
    }

    /// <summary>Picks up what other members changed, including who's in the household and this week's stats.</summary>
    public async Task RefreshAsync(bool notify = true)
    {
        var changes = _changes;
        var tasks = taskApi.GetAllAsync();
        var groceries = groceryApi.GetListAsync();
        var household = householdApi.GetAsync(DueDates.Today);
        await Task.WhenAll(tasks, groceries, household);
        if (changes != _changes)
        {
            await RefreshAsync(notify);
            return;
        }

        Tasks = tasks.Result;
        Groceries = groceries.Result.Items;
        Recents = groceries.Result.Recents;
        Household = household.Result;
        if (notify)
            Notify();
    }

    /// <summary>The household with this week's stats, which change as tasks are done.</summary>
    public async Task RefreshHouseholdAsync()
    {
        Household = await householdApi.GetAsync(DueDates.Today);
        Notify();
    }

    public async Task RenewInviteCodeAsync()
    {
        var code = await householdApi.RenewInviteCodeAsync();
        if (Household is not null)
            Household.InviteCode = code;
        Notify();
    }

    /// <summary>Reloads the household afterwards, since this week's stats count from the new day.</summary>
    public async Task SetWeekStartAsync(DayOfWeek day)
    {
        await householdApi.SetWeekStartAsync(day);
        await RefreshHouseholdAsync();
    }

    /// <summary>Removes the member; their tasks become anyone's, so those are reloaded too.</summary>
    public async Task RemoveMemberAsync(Guid userId)
    {
        await householdApi.RemoveMemberAsync(userId);
        await RefreshAsync();
    }

    /// <summary>Saves the language on the account, so it follows this person to other devices.</summary>
    public Task SetLanguageAsync(string language)
        => authApi.SetLanguageAsync(language);

    public void Clear()
    {
        // Also keeps a refresh that's still on its way from bringing the old account's lists back.
        (Me, Household, Tasks, Groceries, Recents, Loaded) = (null, null, [], [], [], false);
        ChangedHere();
    }

    // ---- Tasks

    public async Task<HouseholdTask> AddTaskAsync(SaveTaskRequest request)
    {
        var task = await taskApi.CreateAsync(request);
        Tasks.Add(task);
        ChangedHere();
        return task;
    }

    public async Task UpdateTaskAsync(Guid id, SaveTaskRequest request)
        => Replace(await taskApi.UpdateAsync(id, request));

    public async Task DeleteTaskAsync(Guid id)
    {
        await taskApi.DeleteAsync(id);
        Tasks.RemoveAll(t => t.Id == id);
        ChangedHere();
    }

    public async Task<CompleteTaskResponse> CompleteTaskAsync(Guid id)
    {
        var response = await taskApi.CompleteAsync(id, DueDates.Today);
        Replace(response.Task);
        return response;
    }

    public async Task UndoCompletionAsync(Guid completionId)
    {
        if (await taskApi.UndoCompletionAsync(completionId) is { } task)
            Replace(task);
    }

    public async Task ReopenTaskAsync(Guid id)
        => Replace(await taskApi.ReopenAsync(id));

    private void Replace(HouseholdTask task)
    {
        var index = Tasks.FindIndex(t => t.Id == task.Id);
        if (index >= 0)
            Tasks[index] = task;
        else
            Tasks.Add(task);
        ChangedHere();
    }

    // ---- Groceries

    public async Task<AddGroceryResponse> AddGroceryAsync(SaveGroceryRequest request)
    {
        var response = await groceryApi.AddAsync(request);
        Replace(response.Item);
        return response;
    }

    public async Task UpdateGroceryAsync(Guid id, SaveGroceryRequest request)
        => Replace(await groceryApi.UpdateAsync(id, request));

    public async Task DeleteGroceryAsync(Guid id)
    {
        await groceryApi.DeleteAsync(id);
        Groceries.RemoveAll(g => g.Id == id);
        ChangedHere();
    }

    /// <summary>Takes the cart off the list; returns the ids, to put them back.</summary>
    public async Task<List<Guid>> ClearCartAsync()
    {
        var ids = await groceryApi.ClearCartAsync();
        _changes++;
        await RefreshAsync();
        return ids;
    }

    public async Task RestoreGroceriesAsync(List<Guid> ids)
    {
        await groceryApi.RestoreAsync(ids);
        _changes++;
        await RefreshAsync();
    }

    private void Replace(GroceryItem item)
    {
        var index = Groceries.FindIndex(g => g.Id == item.Id);
        if (index >= 0)
            Groceries[index] = item;
        else
            Groceries.Add(item);
        ChangedHere();
    }

    private void ChangedHere()
    {
        _changes++;
        Notify();
    }

    private void Notify() => Changed?.Invoke();
}
