using HouseholdPlanner.Api.Data;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Api.Repositories.Implementations;

public class GroceryRepository(AppDbContext context) : IGroceryRepository
{
    public Task<List<GroceryItem>> GetListAsync(Guid householdId)
        => context.GroceryItems
            .Where(g => g.HouseholdId == householdId && g.ClearedAt == null)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync();

    public async Task<List<string>> GetRecentNamesAsync(Guid householdId, int take)
    {
        // Cleared items are purged after a while, so there are never many to go through.
        var cleared = await context.GroceryItems
            .Where(g => g.HouseholdId == householdId && g.ClearedAt != null)
            .OrderByDescending(g => g.ClearedAt)
            .Select(g => g.Name)
            .ToListAsync();
        return cleared.DistinctBy(n => n.ToLowerInvariant()).Take(take).ToList();
    }

    public async Task<EntityResult<GroceryItem>> GetByIdAsync(Guid householdId, Guid id)
    {
        var item = await context.GroceryItems.FirstOrDefaultAsync(g => g.Id == id && g.HouseholdId == householdId && g.ClearedAt == null);
        return item is null
            ? new(null, false, "That item is no longer on the list.", ErrorCode.NotFound)
            : new(item, true, "Item found.");
    }

    public Task<GroceryItem?> FindToBuyAsync(Guid householdId, string name)
    {
        var lower = name.ToLower();
        return context.GroceryItems.FirstOrDefaultAsync(g =>
            g.HouseholdId == householdId && g.ClearedAt == null && !g.InCart && g.Name.ToLower() == lower);
    }

    public Task<List<GroceryItem>> GetInCartAsync(Guid householdId)
        => context.GroceryItems.Where(g => g.HouseholdId == householdId && g.ClearedAt == null && g.InCart).ToListAsync();

    public Task<List<GroceryItem>> GetClearedAsync(Guid householdId, IReadOnlyCollection<Guid> ids)
        => context.GroceryItems.Where(g => g.HouseholdId == householdId && g.ClearedAt != null && ids.Contains(g.Id)).ToListAsync();

    public async Task<GroceryItem> CreateAsync(GroceryItem item)
    {
        context.GroceryItems.Add(item);
        await context.SaveChangesAsync();
        return item;
    }

    public Task SaveAsync() => context.SaveChangesAsync();

    public async Task DeleteAsync(GroceryItem item)
    {
        context.GroceryItems.Remove(item);
        await context.SaveChangesAsync();
    }

    public Task PurgeClearedAsync(Guid householdId, DateTime before)
        => context.GroceryItems
            .Where(g => g.HouseholdId == householdId && g.ClearedAt < before)
            .ExecuteDeleteAsync();
}
