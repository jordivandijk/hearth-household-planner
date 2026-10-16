using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Models;
using HouseholdPlanner.Web.Services;

namespace HouseholdPlanner.Web.Tests;

public class HomeStoreTests
{
    private readonly FakeGroceryApi _groceries = new();
    private readonly HomeStore _store;

    public HomeStoreTests() => _store = new HomeStore(null!, new FakeHouseholdApi(), new FakeTaskApi(), _groceries);

    [Fact]
    public async Task A_refresh_read_before_a_change_does_not_undo_it()
    {
        var milk = _groceries.Add("Milk");
        await _store.RefreshAsync();

        // The 30-second refresh reads the list, but its answer is slow to arrive (a shop with bad reception).
        var slow = _groceries.HoldNextRead();
        var refresh = _store.RefreshAsync();
        await _store.UpdateGroceryAsync(milk.Id, new SaveGroceryRequest { Name = "Milk", InCart = true });
        slow.SetResult();
        await refresh;

        Assert.True(Assert.Single(_store.Groceries).InCart);
    }

    [Fact]
    public async Task A_refresh_picks_up_what_others_changed()
    {
        await _store.RefreshAsync();
        _groceries.Add("Bread");

        await _store.RefreshAsync();

        Assert.Equal("Bread", Assert.Single(_store.Groceries).Name);
    }

    /// <summary>The grocery list on a pretend server. A held read copies the list straight away but answers later.</summary>
    private class FakeGroceryApi : IGroceryApiClient
    {
        private readonly List<GroceryItem> _items = [];
        private TaskCompletionSource? _hold;

        public GroceryItem Add(string name)
        {
            var item = new GroceryItem { Id = Guid.NewGuid(), Name = name };
            _items.Add(item);
            return item;
        }

        /// <summary>Holds back the answer to the next read until the returned source is set.</summary>
        public TaskCompletionSource HoldNextRead() => _hold = new TaskCompletionSource();

        public async Task<GroceryList> GetListAsync()
        {
            var copy = _items.Select(Copy).ToList();
            var hold = _hold;
            _hold = null;
            if (hold is not null)
                await hold.Task;
            return new GroceryList { Items = copy };
        }

        public Task<GroceryItem> UpdateAsync(Guid id, SaveGroceryRequest request)
        {
            var index = _items.FindIndex(i => i.Id == id);
            _items[index] = new GroceryItem { Id = id, Name = request.Name, Quantity = request.Quantity, Category = request.Category, InCart = request.InCart };
            return Task.FromResult(Copy(_items[index]));
        }

        private static GroceryItem Copy(GroceryItem item)
            => new() { Id = item.Id, Name = item.Name, Quantity = item.Quantity, Category = item.Category, InCart = item.InCart };

        public Task<AddGroceryResponse> AddAsync(SaveGroceryRequest request) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id) => throw new NotImplementedException();
        public Task<List<Guid>> ClearCartAsync() => throw new NotImplementedException();
        public Task RestoreAsync(List<Guid> ids) => throw new NotImplementedException();
    }

    private class FakeHouseholdApi : IHouseholdApiClient
    {
        public Task<Household> GetAsync(DateOnly today) => Task.FromResult(new Household());

        public Task<string> RenewInviteCodeAsync() => throw new NotImplementedException();
        public Task SetWeekStartAsync(DayOfWeek day) => throw new NotImplementedException();
        public Task RemoveMemberAsync(Guid userId) => throw new NotImplementedException();
    }

    private class FakeTaskApi : ITaskApiClient
    {
        public Task<List<HouseholdTask>> GetAllAsync() => Task.FromResult(new List<HouseholdTask>());

        public Task<HouseholdTask> CreateAsync(SaveTaskRequest request) => throw new NotImplementedException();
        public Task<HouseholdTask> UpdateAsync(Guid id, SaveTaskRequest request) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id) => throw new NotImplementedException();
        public Task<CompleteTaskResponse> CompleteAsync(Guid id, DateOnly today) => throw new NotImplementedException();
        public Task<HouseholdTask> ReopenAsync(Guid id) => throw new NotImplementedException();
        public Task<HouseholdTask?> UndoCompletionAsync(Guid completionId) => throw new NotImplementedException();
    }
}
