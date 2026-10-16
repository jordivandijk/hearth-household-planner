using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Implementations;

public class TaskApiClient(HttpClient httpClient) : ITaskApiClient
{
    public Task<List<HouseholdTask>> GetAllAsync()
        => httpClient.GetJsonAsync<List<HouseholdTask>>("api/tasks");

    public Task<HouseholdTask> CreateAsync(SaveTaskRequest request)
        => httpClient.SendJsonAsync<HouseholdTask>(HttpMethod.Post, "api/tasks", request);

    public Task<HouseholdTask> UpdateAsync(Guid id, SaveTaskRequest request)
        => httpClient.SendJsonAsync<HouseholdTask>(HttpMethod.Put, $"api/tasks/{id}", request);

    public Task DeleteAsync(Guid id)
        => httpClient.SendJsonAsync(HttpMethod.Delete, $"api/tasks/{id}");

    public Task<CompleteTaskResponse> CompleteAsync(Guid id, DateOnly today)
        => httpClient.SendJsonAsync<CompleteTaskResponse>(HttpMethod.Post, $"api/tasks/{id}/complete", new CompleteTaskRequest { Today = today });

    public Task<HouseholdTask> ReopenAsync(Guid id)
        => httpClient.SendJsonAsync<HouseholdTask>(HttpMethod.Post, $"api/tasks/{id}/reopen");

    public Task<HouseholdTask?> UndoCompletionAsync(Guid completionId)
        => httpClient.SendJsonOrDefaultAsync<HouseholdTask>(HttpMethod.Delete, $"api/tasks/completions/{completionId}");
}
