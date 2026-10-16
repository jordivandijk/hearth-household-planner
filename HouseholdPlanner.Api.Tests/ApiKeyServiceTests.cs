using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Services;
using HouseholdPlanner.Api.Services.Implementations;
using static HouseholdPlanner.Api.Tests.TestData;

namespace HouseholdPlanner.Api.Tests;

public class ApiKeyServiceTests
{
    private readonly FakeApiKeyRepository _keys = new();
    private readonly ApiKeyService _service;
    private readonly User _admin = Member("Sam");

    public ApiKeyServiceTests()
    {
        _admin.Role = UserRole.Admin;
        _service = new ApiKeyService(_keys, new FixedTime(Now));
    }

    [Fact]
    public async Task A_new_key_is_shown_once_and_only_its_hash_is_stored()
    {
        var result = await _service.CreateAsync(_admin.Id, new CreateApiKeyRequest { Name = "Home Assistant" });

        Assert.True(result.Success);
        var secret = result.Data.Key;
        Assert.StartsWith(ApiKeySecrets.Start, secret);

        var stored = Assert.Single(_keys.Keys);
        Assert.Equal(ApiKeySecrets.Hash(secret), stored.Hash);
        Assert.DoesNotContain(secret, stored.Hash);
        Assert.Equal(secret[..ApiKeySecrets.PrefixLength], stored.Prefix);
        Assert.Equal(Now.UtcDateTime, stored.CreatedAt);
        Assert.Equal(_admin.Id, stored.UserId);
    }

    [Fact]
    public async Task The_name_is_trimmed()
    {
        var result = await _service.CreateAsync(_admin.Id, new CreateApiKeyRequest { Name = "  n8n " });

        Assert.Equal("n8n", result.Data.ApiKey.Name);
    }

    [Fact]
    public async Task A_key_needs_a_name()
    {
        var result = await _service.CreateAsync(_admin.Id, new CreateApiKeyRequest { Name = "   " });

        Assert.Equal(ErrorCode.InvalidInput, result.ErrorCode);
        Assert.Empty(_keys.Keys);
    }

    [Fact]
    public async Task Every_key_is_different()
    {
        var first = await _service.CreateAsync(_admin.Id, new CreateApiKeyRequest { Name = "One" });
        var second = await _service.CreateAsync(_admin.Id, new CreateApiKeyRequest { Name = "Two" });

        Assert.NotEqual(first.Data.Key, second.Data.Key);
    }

    [Fact]
    public async Task The_list_has_only_your_own_keys()
    {
        await _service.CreateAsync(_admin.Id, new CreateApiKeyRequest { Name = "Home Assistant" });
        await _service.CreateAsync(Guid.NewGuid(), new CreateApiKeyRequest { Name = "Someone else's" });

        var keys = await _service.GetAllAsync(_admin.Id);

        Assert.Equal("Home Assistant", Assert.Single(keys).Name);
    }

    [Fact]
    public async Task A_revoked_key_is_deleted()
    {
        var created = await _service.CreateAsync(_admin.Id, new CreateApiKeyRequest { Name = "n8n" });

        var result = await _service.RevokeAsync(_admin.Id, created.Data.ApiKey.Id);

        Assert.True(result.Success);
        Assert.Empty(_keys.Keys);
    }

    [Fact]
    public async Task You_cannot_revoke_someone_else_s_key()
    {
        var created = await _service.CreateAsync(Guid.NewGuid(), new CreateApiKeyRequest { Name = "Theirs" });

        var result = await _service.RevokeAsync(_admin.Id, created.Data.ApiKey.Id);

        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
        Assert.Single(_keys.Keys);
    }
}
