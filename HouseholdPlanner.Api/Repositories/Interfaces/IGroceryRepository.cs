using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Repositories.Interfaces;

public interface IGroceryRepository
{
    /// <summary>What's on the list now: to buy and in the cart, not cleared.</summary>
    Task<List<GroceryItem>> GetListAsync(Guid householdId);
    /// <summary>Distinct names of cleared items, most recently bought first.</summary>
    Task<List<string>> GetRecentNamesAsync(Guid householdId, int take);
    Task<EntityResult<GroceryItem>> GetByIdAsync(Guid householdId, Guid id);
    /// <summary>The item with this name that is still to buy, ignoring case.</summary>
    Task<GroceryItem?> FindToBuyAsync(Guid householdId, string name);
    Task<List<GroceryItem>> GetInCartAsync(Guid householdId);
    Task<List<GroceryItem>> GetClearedAsync(Guid householdId, IReadOnlyCollection<Guid> ids);
    Task<GroceryItem> CreateAsync(GroceryItem item);
    /// <summary>Saves changes to tracked items.</summary>
    Task SaveAsync();
    Task DeleteAsync(GroceryItem item);
    /// <summary>Forgets cleared items bought before <paramref name="before"/>.</summary>
    Task PurgeClearedAsync(Guid householdId, DateTime before);
}
