using HouseholdPlanner.Api.Data;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Api.Repositories.Implementations;

public class ApiKeyRepository(AppDbContext context) : IApiKeyRepository
{
    public Task<List<ApiKey>> GetForUserAsync(Guid userId)
        => context.ApiKeys.Where(k => k.UserId == userId).OrderBy(k => k.CreatedAt).ToListAsync();

    public async Task<EntityResult<ApiKey>> GetByIdAsync(Guid userId, Guid id)
    {
        var key = await context.ApiKeys.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);
        return key is null
            ? new(null, false, "That API key no longer exists.", ErrorCode.NotFound)
            : new(key, true, "API key found.");
    }

    public async Task<EntityResult<ApiKey>> CreateAsync(ApiKey key)
    {
        context.ApiKeys.Add(key);
        await context.SaveChangesAsync();
        return new(key, true, "API key created.");
    }

    public async Task DeleteAsync(ApiKey key)
    {
        context.ApiKeys.Remove(key);
        await context.SaveChangesAsync();
    }
}
