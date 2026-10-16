using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Interfaces;

/// <summary>Admins: keys for automations like Home Assistant or n8n.</summary>
public interface IApiKeyApiClient
{
    Task<List<ApiKey>> GetAllAsync();
    /// <summary>Makes a key. The result holds the key itself, which can't be fetched again.</summary>
    Task<CreatedApiKey> CreateAsync(string name);
    Task RevokeAsync(Guid id);
}
