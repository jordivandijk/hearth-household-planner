using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Repositories.Interfaces;

public interface IApiKeyRepository
{
    /// <summary>The user's keys, oldest first.</summary>
    Task<List<ApiKey>> GetForUserAsync(Guid userId);
    /// <summary>The key, if it's this user's.</summary>
    Task<EntityResult<ApiKey>> GetByIdAsync(Guid userId, Guid id);
    Task<EntityResult<ApiKey>> CreateAsync(ApiKey key);
    Task DeleteAsync(ApiKey key);
}
