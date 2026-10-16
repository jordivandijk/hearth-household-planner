using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Services.Interfaces;

/// <summary>The admin's keys for automations. A key can do everything its admin can, apart from managing keys.</summary>
public interface IApiKeyService
{
    Task<List<ApiKeyResponse>> GetAllAsync(Guid userId);
    /// <summary>Makes a key. The response holds the key itself, which can't be shown again.</summary>
    Task<ValueResult<CreatedApiKeyResponse>> CreateAsync(Guid userId, CreateApiKeyRequest request);
    /// <summary>Deletes the key; anything using it is refused from its next request.</summary>
    Task<Result> RevokeAsync(Guid userId, Guid id);
}
