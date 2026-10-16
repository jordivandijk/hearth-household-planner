using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services.Interfaces;

namespace HouseholdPlanner.Api.Services.Implementations;

public class HouseholdService(IHouseholdRepository householdRepository, IUserRepository userRepository, ITaskRepository taskRepository, TimeProvider time) : IHouseholdService
{
    public async Task<ValueResult<HouseholdResponse>> GetAsync(Guid householdId, DateOnly today)
    {
        if (!ClientDates.IsPlausible(today, time.GetUtcNow()))
            return new(ClientDates.WrongClockMessage, false, new(), ErrorCode.InvalidInput);

        var result = await householdRepository.GetByIdAsync(householdId);
        if (!result.Success)
            return new(result.Message, false, new(), result.ErrorCode);

        var members = await userRepository.GetByHouseholdAsync(householdId);
        var done    = await taskRepository.CountCompletionsByUserAsync(householdId, WeekStart(today, result.Data!.WeekStartsOn));
        var open    = await taskRepository.CountOpenByAssigneeAsync(householdId);

        return new("Household found.", true, new HouseholdResponse
        {
            Id           = result.Data.Id,
            Name         = result.Data.Name,
            InviteCode   = result.Data.InviteCode,
            WeekStartsOn = result.Data.WeekStartsOn,
            Members      = members.Select(m => new MemberResponse
            {
                Id           = m.Id,
                Username     = m.Username,
                Hue          = m.Hue,
                Role         = m.Role,
                DoneThisWeek = done.GetValueOrDefault(m.Id),
                Open         = open.GetValueOrDefault(m.Id)
            }).ToList()
        });
    }

    public async Task<ValueResult<string>> RenewInviteCodeAsync(Guid householdId)
    {
        var result = await householdRepository.GetByIdAsync(householdId);
        if (!result.Success)
            return new(result.Message, false, "", result.ErrorCode);

        result.Data!.InviteCode = InviteCodes.Generate();
        await householdRepository.SaveAsync();
        return new("Invite code renewed.", true, result.Data.InviteCode);
    }

    public async Task<Result> SetWeekStartAsync(Guid householdId, SetWeekStartRequest request)
    {
        var result = await householdRepository.GetByIdAsync(householdId);
        if (!result.Success)
            return new(result.Message, false, result.ErrorCode);

        result.Data!.WeekStartsOn = request.WeekStartsOn!.Value;
        await householdRepository.SaveAsync();
        return new("Week start saved.", true);
    }

    /// <summary>The first day of the calendar week that <paramref name="date"/> is in.</summary>
    private static DateOnly WeekStart(DateOnly date, DayOfWeek firstDay)
        => date.AddDays(-(((int)date.DayOfWeek - (int)firstDay + 7) % 7));
}
