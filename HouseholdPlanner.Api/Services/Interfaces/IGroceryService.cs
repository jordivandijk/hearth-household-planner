using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Services.Interfaces;

public interface IGroceryService
{
    Task<ValueResult<GroceryListResponse>> GetListAsync(Guid householdId);
    /// <summary>Adds the item, or raises the quantity when the same name is still to buy.</summary>
    Task<ValueResult<AddGroceryResponse>> AddAsync(Guid householdId, SaveGroceryRequest request);
    Task<ValueResult<GroceryResponse>> UpdateAsync(Guid householdId, Guid id, SaveGroceryRequest request);
    Task<Result> DeleteAsync(Guid householdId, Guid id);
    /// <summary>Takes everything in the cart off the list. Returns the ids, to restore them.</summary>
    Task<ValueResult<List<Guid>>> ClearCartAsync(Guid householdId);
    /// <summary>Puts cleared items back in the cart.</summary>
    Task<Result> RestoreAsync(Guid householdId, GroceryIdsRequest request);
}
