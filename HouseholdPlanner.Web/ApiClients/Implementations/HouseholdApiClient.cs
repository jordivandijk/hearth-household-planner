using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Implementations;

public class HouseholdApiClient(HttpClient httpClient) : IHouseholdApiClient
{
    public Task<Household> GetAsync(DateOnly today)
        => httpClient.GetJsonAsync<Household>($"api/household?today={today:yyyy-MM-dd}");

    public async Task<string> RenewInviteCodeAsync()
        => (await httpClient.SendJsonAsync<InviteCodeResponse>(HttpMethod.Post, "api/household/invite-code")).InviteCode;

    public Task SetWeekStartAsync(DayOfWeek day)
        => httpClient.SendJsonAsync(HttpMethod.Put, "api/household/week-start", new SetWeekStartRequest { WeekStartsOn = day });

    public Task RemoveMemberAsync(Guid userId)
        => httpClient.SendJsonAsync(HttpMethod.Delete, $"api/users/{userId}");
}
