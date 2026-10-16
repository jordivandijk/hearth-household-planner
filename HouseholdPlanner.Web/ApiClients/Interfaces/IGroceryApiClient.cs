using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Interfaces;

public interface IGroceryApiClient
{
    Task<GroceryList> GetListAsync();
    Task<AddGroceryResponse> AddAsync(SaveGroceryRequest request);
    Task<GroceryItem> UpdateAsync(Guid id, SaveGroceryRequest request);
    Task DeleteAsync(Guid id);
    /// <summary>Takes everything in the cart off the list; returns the ids, to restore them.</summary>
    Task<List<Guid>> ClearCartAsync();
    Task RestoreAsync(List<Guid> ids);
}
