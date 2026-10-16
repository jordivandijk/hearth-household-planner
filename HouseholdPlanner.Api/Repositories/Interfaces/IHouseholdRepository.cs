using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;

namespace HouseholdPlanner.Api.Repositories.Interfaces;

public interface IHouseholdRepository
{
    Task<EntityResult<Household>> GetByIdAsync(Guid id);
    /// <summary>The household with this invite code, ignoring case and surrounding spaces.</summary>
    Task<EntityResult<Household>> GetByInviteCodeAsync(string inviteCode);
    /// <summary>Saves changes to a tracked household.</summary>
    Task SaveAsync();
}
