using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services.Interfaces;

namespace HouseholdPlanner.Api.Services.Implementations;

public class GroceryService(IGroceryRepository groceryRepository, TimeProvider time) : IGroceryService
{
    private const int RecentsShown = 14;
    private static readonly TimeSpan ClearedKept = TimeSpan.FromDays(90);

    public async Task<ValueResult<GroceryListResponse>> GetListAsync(Guid householdId)
    {
        var items   = await groceryRepository.GetListAsync(householdId);
        var recents = await groceryRepository.GetRecentNamesAsync(householdId, RecentsShown);
        return new("List retrieved.", true, new GroceryListResponse
        {
            Items   = items.Select(ToResponse).ToList(),
            Recents = recents
        });
    }

    public async Task<ValueResult<AddGroceryResponse>> AddAsync(Guid householdId, SaveGroceryRequest request)
    {
        var name = request.Name.Trim();
        if (!request.InCart && await groceryRepository.FindToBuyAsync(householdId, name) is { } existing)
        {
            var previous = existing.Quantity;
            existing.Quantity = Math.Min(GroceryItem.MaxQuantity, existing.Quantity + request.Quantity);
            await groceryRepository.SaveAsync();
            return new("Quantity raised.", true, new AddGroceryResponse { Item = ToResponse(existing), Merged = true, PreviousQuantity = previous });
        }

        var item = await groceryRepository.CreateAsync(new GroceryItem
        {
            Id          = Guid.NewGuid(),
            HouseholdId = householdId,
            Name        = name,
            Quantity    = request.Quantity,
            Category    = request.Category,
            InCart      = request.InCart
        });
        return new("Item added.", true, new AddGroceryResponse { Item = ToResponse(item) });
    }

    public async Task<ValueResult<GroceryResponse>> UpdateAsync(Guid householdId, Guid id, SaveGroceryRequest request)
    {
        var result = await groceryRepository.GetByIdAsync(householdId, id);
        if (!result.Success)
            return new(result.Message, false, new(), result.ErrorCode);

        var item = result.Data!;
        item.Name     = request.Name.Trim();
        item.Quantity = request.Quantity;
        item.Category = request.Category;
        item.InCart   = request.InCart;
        await groceryRepository.SaveAsync();
        return new("Item updated.", true, ToResponse(item));
    }

    public async Task<Result> DeleteAsync(Guid householdId, Guid id)
    {
        var result = await groceryRepository.GetByIdAsync(householdId, id);
        if (!result.Success)
            return result;

        await groceryRepository.DeleteAsync(result.Data!);
        return new("Item deleted.", true);
    }

    public async Task<ValueResult<List<Guid>>> ClearCartAsync(Guid householdId)
    {
        var now   = time.GetUtcNow().UtcDateTime;
        var items = await groceryRepository.GetInCartAsync(householdId);
        foreach (var item in items)
            item.ClearedAt = now;
        await groceryRepository.SaveAsync();
        await groceryRepository.PurgeClearedAsync(householdId, now - ClearedKept);
        return new("Cart cleared.", true, items.Select(i => i.Id).ToList());
    }

    public async Task<Result> RestoreAsync(Guid householdId, GroceryIdsRequest request)
    {
        var items = await groceryRepository.GetClearedAsync(householdId, request.Ids);
        foreach (var item in items)
            item.ClearedAt = null;
        await groceryRepository.SaveAsync();
        return new("Items restored.", true);
    }

    private static GroceryResponse ToResponse(GroceryItem item) => new()
    {
        Id       = item.Id,
        Name     = item.Name,
        Quantity = item.Quantity,
        Category = item.Category,
        InCart   = item.InCart
    };
}
