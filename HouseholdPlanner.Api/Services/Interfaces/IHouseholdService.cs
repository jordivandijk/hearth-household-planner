using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Services.Interfaces;

public interface IHouseholdService
{
    /// <summary>The household with its members and how much each did this calendar week, up to <paramref name="today"/>.</summary>
    Task<ValueResult<HouseholdResponse>> GetAsync(Guid householdId, DateOnly today);
    /// <summary>Replaces the invite code, so the old one stops working. Returns the new code.</summary>
    Task<ValueResult<string>> RenewInviteCodeAsync(Guid householdId);
    /// <summary>Sets the day the household's weeks start on, for its task list and its stats.</summary>
    Task<Result> SetWeekStartAsync(Guid householdId, SetWeekStartRequest request);
}
