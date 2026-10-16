using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Implementations;

public class ApiKeyApiClient(HttpClient httpClient) : IApiKeyApiClient
{
    public Task<List<ApiKey>> GetAllAsync()
        => httpClient.GetJsonAsync<List<ApiKey>>("api/api-keys");

    public Task<CreatedApiKey> CreateAsync(string name)
        => httpClient.SendJsonAsync<CreatedApiKey>(HttpMethod.Post, "api/api-keys", new CreateApiKeyRequest { Name = name });

    public Task RevokeAsync(Guid id)
        => httpClient.SendJsonAsync(HttpMethod.Delete, $"api/api-keys/{id}");
}
