using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services.Interfaces;

namespace HouseholdPlanner.Api.Services.Implementations;

public class ApiKeyService(IApiKeyRepository apiKeyRepository, TimeProvider time) : IApiKeyService
{
    public async Task<List<ApiKeyResponse>> GetAllAsync(Guid userId)
        => [.. (await apiKeyRepository.GetForUserAsync(userId)).Select(ToResponse)];

    public async Task<ValueResult<CreatedApiKeyResponse>> CreateAsync(Guid userId, CreateApiKeyRequest request)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
            return new("Give the key a name.", false, new(), ErrorCode.InvalidInput);

        var secret = ApiKeySecrets.Generate();
        var result = await apiKeyRepository.CreateAsync(new ApiKey
        {
            Id        = Guid.NewGuid(),
            Name      = name,
            Prefix    = ApiKeySecrets.Prefix(secret),
            Hash      = ApiKeySecrets.Hash(secret),
            CreatedAt = time.GetUtcNow().UtcDateTime,
            UserId    = userId
        });
        if (!result.Success)
            return new(result.Message, false, new(), result.ErrorCode);

        return new("API key created.", true, new CreatedApiKeyResponse { ApiKey = ToResponse(result.Data!), Key = secret });
    }

    public async Task<Result> RevokeAsync(Guid userId, Guid id)
    {
        var result = await apiKeyRepository.GetByIdAsync(userId, id);
        if (!result.Success)
            return new(result.Message, false, result.ErrorCode);

        await apiKeyRepository.DeleteAsync(result.Data!);
        return new($"{result.Data!.Name} was revoked.", true);
    }

    private static ApiKeyResponse ToResponse(ApiKey key) => new()
    {
        Id         = key.Id,
        Name       = key.Name,
        Prefix     = key.Prefix,
        CreatedAt  = key.CreatedAt,
        LastUsedAt = key.LastUsedAt
    };
}
