using HouseholdPlanner.Web.Models;

namespace HouseholdPlanner.Web.ApiClients.Interfaces;

public interface IHouseholdApiClient
{
    Task<Household> GetAsync(DateOnly today);
    /// <summary>Admins: replaces the invite code. Returns the new one.</summary>
    Task<string> RenewInviteCodeAsync();
    /// <summary>Admins: sets the day the household's weeks start on.</summary>
    Task SetWeekStartAsync(DayOfWeek day);
    /// <summary>Admins: removes someone from the household and deletes their account.</summary>
    Task RemoveMemberAsync(Guid userId);
}
