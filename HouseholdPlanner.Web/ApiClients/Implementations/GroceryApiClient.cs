using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Implementations;

public class GroceryApiClient(HttpClient httpClient) : IGroceryApiClient
{
    public Task<GroceryList> GetListAsync()
        => httpClient.GetJsonAsync<GroceryList>("api/groceries");

    public Task<AddGroceryResponse> AddAsync(SaveGroceryRequest request)
        => httpClient.SendJsonAsync<AddGroceryResponse>(HttpMethod.Post, "api/groceries", request);

    public Task<GroceryItem> UpdateAsync(Guid id, SaveGroceryRequest request)
        => httpClient.SendJsonAsync<GroceryItem>(HttpMethod.Put, $"api/groceries/{id}", request);

    public Task DeleteAsync(Guid id)
        => httpClient.SendJsonAsync(HttpMethod.Delete, $"api/groceries/{id}");

    public Task<List<Guid>> ClearCartAsync()
        => httpClient.SendJsonAsync<List<Guid>>(HttpMethod.Post, "api/groceries/clear-cart");

    public Task RestoreAsync(List<Guid> ids)
        => httpClient.SendJsonAsync(HttpMethod.Post, "api/groceries/restore", new GroceryIdsRequest { Ids = ids });
}
