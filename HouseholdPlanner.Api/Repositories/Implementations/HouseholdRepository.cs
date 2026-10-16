using HouseholdPlanner.Api.Data;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Api.Repositories.Implementations;

public class HouseholdRepository(AppDbContext context) : IHouseholdRepository
{
    public async Task<EntityResult<Household>> GetByIdAsync(Guid id)
    {
        var household = await context.Households.FindAsync(id);
        return household is null
            ? new(null, false, "Household not found.", ErrorCode.NotFound)
            : new(household, true, "Household found.");
    }

    public async Task<EntityResult<Household>> GetByInviteCodeAsync(string inviteCode)
    {
        var code = InviteCodes.Normalize(inviteCode);
        var household = await context.Households.FirstOrDefaultAsync(h => h.InviteCode == code);
        return household is null
            ? new(null, false, "That invite code doesn't match a household.", ErrorCode.NotFound)
            : new(household, true, "Household found.");
    }

    public Task SaveAsync() => context.SaveChangesAsync();
}
