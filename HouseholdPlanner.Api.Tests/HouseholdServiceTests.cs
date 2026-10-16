using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Services.Implementations;
using static HouseholdPlanner.Api.Tests.TestData;

namespace HouseholdPlanner.Api.Tests;

public class HouseholdServiceTests
{
    private readonly FakeHouseholdRepository _households = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeTaskRepository _tasks = new();
    private readonly HouseholdService _service;
    private readonly User _sam = Member("Sam");
    private readonly User _june = Member("June");

    public HouseholdServiceTests()
    {
        _households.Households.Add(new Household { Id = HouseholdId, Name = "Home", InviteCode = "KQTM-4821" });
        _users.Users.AddRange([_sam, _june, Member("Theirs", OtherHouseholdId)]);
        _service = new HouseholdService(_households, _users, _tasks, new FixedTime(Now));
    }

    private void Done(User user, DateOnly on, Guid? householdId = null)
        => _tasks.Completions.Add(new TaskCompletion { Id = Guid.NewGuid(), HouseholdId = householdId ?? HouseholdId, UserId = user.Id, On = on });

    [Fact]
    public async Task Counts_what_each_member_did_this_calendar_week()
    {
        Done(_sam, Today);
        Done(_sam, Today.AddDays(-3));   // Monday: the first day of this week
        Done(_sam, Today.AddDays(-4));   // Sunday: last week
        Done(_june, Today.AddDays(-1));
        Done(_june, Today, OtherHouseholdId);

        var result = await _service.GetAsync(HouseholdId, Today);

        var members = result.Data.Members.ToDictionary(m => m.Username, m => m.DoneThisWeek);
        Assert.Equal(new Dictionary<string, int> { ["Sam"] = 2, ["June"] = 1 }, members);
    }

    [Fact]
    public async Task This_weeks_count_starts_on_the_households_day()
    {
        _households.Households.Single().WeekStartsOn = DayOfWeek.Sunday;
        Done(_sam, Today.AddDays(-4));   // Sunday: now the first day of this week
        Done(_sam, Today.AddDays(-5));   // Saturday: last week

        var result = await _service.GetAsync(HouseholdId, Today);

        Assert.Equal(1, result.Data.Members.Single(m => m.Id == _sam.Id).DoneThisWeek);
    }

    [Fact]
    public async Task Counts_open_tasks_per_assignee()
    {
        _tasks.Tasks.Add(Chore("Vacuum", assigneeId: _june.Id));
        _tasks.Tasks.Add(Chore("Dishes", assigneeId: _june.Id));
        _tasks.Tasks.Add(Chore("Anyone"));
        var done = Chore("Done", assigneeId: _june.Id);
        done.Done = true;
        _tasks.Tasks.Add(done);

        var result = await _service.GetAsync(HouseholdId, Today);

        Assert.Equal(2, result.Data.Members.Single(m => m.Id == _june.Id).Open);
        Assert.Equal(0, result.Data.Members.Single(m => m.Id == _sam.Id).Open);
    }

    [Fact]
    public async Task Members_come_with_their_role()
    {
        _sam.Role = UserRole.Admin;

        var result = await _service.GetAsync(HouseholdId, Today);

        Assert.Equal(UserRole.Admin, result.Data.Members.Single(m => m.Id == _sam.Id).Role);
        Assert.Equal(UserRole.Member, result.Data.Members.Single(m => m.Id == _june.Id).Role);
    }

    [Fact]
    public async Task A_new_invite_code_replaces_the_old_one()
    {
        var household = _households.Households.Single();
        var old = household.InviteCode;

        var result = await _service.RenewInviteCodeAsync(HouseholdId);

        Assert.True(result.Success);
        Assert.NotEqual(old, result.Data);
        Assert.Equal(result.Data, household.InviteCode);
        Assert.False((await _households.GetByInviteCodeAsync(old)).Success);
    }

    [Fact]
    public async Task Weeks_start_on_monday_until_the_admin_picks_another_day()
    {
        Assert.Equal(DayOfWeek.Monday, (await _service.GetAsync(HouseholdId, Today)).Data.WeekStartsOn);

        var result = await _service.SetWeekStartAsync(HouseholdId, new SetWeekStartRequest { WeekStartsOn = DayOfWeek.Sunday });

        Assert.True(result.Success);
        Assert.Equal(DayOfWeek.Sunday, (await _service.GetAsync(HouseholdId, Today)).Data.WeekStartsOn);
    }

    [Fact]
    public async Task A_missing_today_is_rejected_instead_of_crashing()
    {
        var result = await _service.GetAsync(HouseholdId, default);

        Assert.Equal(ErrorCode.InvalidInput, result.ErrorCode);
    }
}
