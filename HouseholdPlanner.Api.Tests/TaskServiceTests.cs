using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Services.Implementations;
using static HouseholdPlanner.Api.Tests.TestData;

namespace HouseholdPlanner.Api.Tests;

public class TaskServiceTests
{
    private readonly FakeTaskRepository _tasks = new();
    private readonly FakeUserRepository _users = new();
    private readonly FixedTime _time = new(Now);
    private readonly User _june = Member("June");
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _users.Users.Add(_june);
        _service = new TaskService(_tasks, _users, _time);
    }

    private HouseholdTask Add(HouseholdTask task)
    {
        _tasks.Tasks.Add(task);
        return task;
    }

    private Task<ValueResult<CompleteTaskResponse>> Complete(HouseholdTask task, DateOnly? today = null)
        => _service.CompleteAsync(HouseholdId, _june.Id, task.Id, new CompleteTaskRequest { Today = today ?? Today });

    [Fact]
    public async Task Completing_a_one_off_task_marks_it_done_by_the_user()
    {
        var task = Add(Chore("Book plumber", Today));

        var result = await Complete(task);

        Assert.True(result.Success);
        Assert.True(task.Done);
        Assert.Equal(_june.Id, task.DoneById);
        Assert.Equal(Now.UtcDateTime, task.DoneAt);
        var completion = Assert.Single(_tasks.Completions);
        Assert.Equal((_june.Id, Today, (DateOnly?)Today), (completion.UserId, completion.On, completion.PreviousDue));
        Assert.Equal(completion.Id, result.Data.CompletionId);
    }

    [Fact]
    public async Task Completing_a_repeating_task_keeps_it_open_and_moves_it_on_from_today()
    {
        var task = Add(Chore("Take out recycling", Today.AddDays(-3), TaskRepeat.Weekly));

        var result = await Complete(task);

        Assert.True(result.Success);
        Assert.False(task.Done);
        Assert.Equal(Today.AddDays(7), task.Due);
        Assert.Equal(Today.AddDays(-3), Assert.Single(_tasks.Completions).PreviousDue);
    }

    [Fact]
    public async Task Completing_a_biweekly_task_brings_it_back_in_two_weeks()
    {
        var task = Add(Chore("Change bed sheets", Today, TaskRepeat.Biweekly));

        await Complete(task);

        Assert.False(task.Done);
        Assert.Equal(Today.AddDays(14), task.Due);
    }

    [Fact]
    public async Task A_done_task_cannot_be_completed_again()
    {
        var task = Add(Chore("Book plumber", Today));
        await Complete(task);

        var again = await Complete(task);

        Assert.Equal(ErrorCode.Conflict, again.ErrorCode);
        Assert.Single(_tasks.Completions);
    }

    [Fact]
    public async Task Another_households_task_is_not_found()
    {
        var task = Add(Chore("Theirs", Today, householdId: OtherHouseholdId));

        var result = await Complete(task);

        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
        Assert.False(task.Done);
    }

    [Theory]
    [InlineData(-1, true)]   // UTC+ zones are a day ahead
    [InlineData(1, true)]    // UTC− zones a day behind
    [InlineData(2, false)]
    [InlineData(-400, false)]
    public async Task Completing_only_accepts_a_today_within_a_day_of_the_server_date(int offset, bool accepted)
    {
        var task = Add(Chore("Water the plants", Today, TaskRepeat.Daily));

        var result = await Complete(task, Today.AddDays(offset));

        Assert.Equal(accepted, result.Success);
        if (!accepted)
            Assert.Equal(ErrorCode.InvalidInput, result.ErrorCode);
    }

    [Fact]
    public async Task A_missing_today_is_rejected_instead_of_logging_year_one()
    {
        var task = Add(Chore("Water the plants", Today, TaskRepeat.Monthly));

        var result = await Complete(task, default(DateOnly));

        Assert.Equal(ErrorCode.InvalidInput, result.ErrorCode);
        Assert.Empty(_tasks.Completions);
    }

    [Fact]
    public async Task Undoing_a_completion_restores_the_task_and_drops_the_log_entry()
    {
        var oneOff = Add(Chore("Book plumber", Today));
        var weekly = Add(Chore("Take out recycling", Today.AddDays(-1), TaskRepeat.Weekly));
        var first = await Complete(oneOff);
        var second = await Complete(weekly);

        await _service.UndoCompletionAsync(HouseholdId, first.Data.CompletionId);
        var undone = await _service.UndoCompletionAsync(HouseholdId, second.Data.CompletionId);

        Assert.False(oneOff.Done);
        Assert.Null(oneOff.DoneById);
        Assert.Null(oneOff.DoneAt);
        Assert.Equal(Today.AddDays(-1), weekly.Due);
        Assert.Equal(Today.AddDays(-1), undone.Data!.Due);
        Assert.Empty(_tasks.Completions);
    }

    [Fact]
    public async Task Undoing_the_completion_of_a_deleted_task_only_drops_the_log_entry()
    {
        var task = Add(Chore("Book plumber", Today));
        var completed = await Complete(task);
        await _service.DeleteAsync(HouseholdId, task.Id);

        var undone = await _service.UndoCompletionAsync(HouseholdId, completed.Data.CompletionId);

        Assert.True(undone.Success);
        Assert.Null(undone.Data);
        Assert.Empty(_tasks.Completions);
    }

    [Fact]
    public async Task Another_households_completion_cannot_be_undone()
    {
        var task = Add(Chore("Book plumber", Today));
        var completed = await Complete(task);

        var result = await _service.UndoCompletionAsync(OtherHouseholdId, completed.Data.CompletionId);

        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
        Assert.True(task.Done);
    }

    [Fact]
    public async Task Reopening_a_done_task_clears_who_did_it()
    {
        var task = Add(Chore("Book plumber", Today));
        await Complete(task);

        var result = await _service.ReopenAsync(HouseholdId, task.Id);

        Assert.True(result.Success);
        Assert.False(task.Done);
        Assert.Null(task.DoneById);
        Assert.Null(task.DoneAt);
    }

    [Fact]
    public async Task Reopening_takes_back_the_completion_so_ticking_again_counts_once()
    {
        var other = Add(Chore("Water the plants", Today, TaskRepeat.Daily));
        var task = Add(Chore("Book plumber", Today));
        await Complete(other);
        await Complete(task);

        await _service.ReopenAsync(HouseholdId, task.Id);
        await Complete(task);

        Assert.Equal(2, _tasks.Completions.Count);
        Assert.Single(_tasks.Completions, c => c.TaskId == task.Id);
    }

    [Fact]
    public async Task Reopening_an_open_task_keeps_its_completions()
    {
        var task = Add(Chore("Water the plants", Today, TaskRepeat.Daily));
        await Complete(task);

        var result = await _service.ReopenAsync(HouseholdId, task.Id);

        Assert.True(result.Success);
        Assert.Single(_tasks.Completions);
    }

    [Fact]
    public async Task A_task_can_only_be_assigned_to_someone_in_the_household()
    {
        var outsider = Member("Rio", OtherHouseholdId);
        _users.Users.Add(outsider);

        var mine = await _service.CreateAsync(HouseholdId, new SaveTaskRequest { Title = "Vacuum", AssigneeId = _june.Id });
        var theirs = await _service.CreateAsync(HouseholdId, new SaveTaskRequest { Title = "Vacuum", AssigneeId = outsider.Id });
        var unknown = await _service.CreateAsync(HouseholdId, new SaveTaskRequest { Title = "Vacuum", AssigneeId = Guid.NewGuid() });

        Assert.True(mine.Success);
        Assert.Equal(ErrorCode.InvalidInput, theirs.ErrorCode);
        Assert.Equal(ErrorCode.InvalidInput, unknown.ErrorCode);
        Assert.Single(_tasks.Tasks);
    }

    [Fact]
    public async Task Creating_a_task_trims_the_title_and_allows_anyone()
    {
        var result = await _service.CreateAsync(HouseholdId, new SaveTaskRequest { Title = "  Fix chair  ", AssigneeId = null });

        Assert.Equal("Fix chair", result.Data.Title);
        Assert.Null(result.Data.AssigneeId);
        Assert.Equal(HouseholdId, Assert.Single(_tasks.Tasks).HouseholdId);
    }

    [Fact]
    public async Task Another_households_task_cannot_be_edited_or_deleted()
    {
        var task = Add(Chore("Theirs", householdId: OtherHouseholdId));

        var update = await _service.UpdateAsync(HouseholdId, task.Id, new SaveTaskRequest { Title = "Mine now" });
        var delete = await _service.DeleteAsync(HouseholdId, task.Id);

        Assert.Equal(ErrorCode.NotFound, update.ErrorCode);
        Assert.Equal(ErrorCode.NotFound, delete.ErrorCode);
        Assert.Equal("Theirs", task.Title);
        Assert.Contains(task, _tasks.Tasks);
    }

    [Fact]
    public async Task The_list_shows_open_tasks_and_ones_done_in_the_last_30_days()
    {
        Add(Chore("Open"));
        var recent = Add(Chore("Recent"));
        var old = Add(Chore("Old"));
        (recent.Done, recent.DoneAt) = (true, Now.UtcDateTime.AddDays(-29));
        (old.Done, old.DoneAt) = (true, Now.UtcDateTime.AddDays(-31));

        var result = await _service.GetAllAsync(HouseholdId);

        Assert.Equal(["Open", "Recent"], result.Data.Select(t => t.Title).Order());
    }
}
