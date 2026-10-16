using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Repositories.Interfaces;
using HouseholdPlanner.Api.Services.Interfaces;

namespace HouseholdPlanner.Api.Services.Implementations;

public class TaskService(ITaskRepository taskRepository, IUserRepository userRepository, TimeProvider time) : ITaskService
{
    private static readonly TimeSpan DoneTasksShown = TimeSpan.FromDays(30);

    public async Task<ValueResult<List<TaskResponse>>> GetAllAsync(Guid householdId)
    {
        var tasks = await taskRepository.GetForHouseholdAsync(householdId, time.GetUtcNow().UtcDateTime - DoneTasksShown);
        return new("Tasks retrieved.", true, tasks.Select(ToResponse).ToList());
    }

    public async Task<ValueResult<TaskResponse>> CreateAsync(Guid householdId, SaveTaskRequest request)
    {
        if (!await IsMemberAsync(householdId, request.AssigneeId))
            return new("That person isn't in your household.", false, new(), ErrorCode.InvalidInput);

        var task = await taskRepository.CreateAsync(new HouseholdTask
        {
            Id          = Guid.NewGuid(),
            HouseholdId = householdId,
            Title       = request.Title.Trim(),
            AssigneeId  = request.AssigneeId,
            Due         = request.Due,
            Repeat      = request.Repeat
        });
        return new("Task created.", true, ToResponse(task));
    }

    public async Task<ValueResult<TaskResponse>> UpdateAsync(Guid householdId, Guid id, SaveTaskRequest request)
    {
        var result = await taskRepository.GetByIdAsync(householdId, id);
        if (!result.Success)
            return new(result.Message, false, new(), result.ErrorCode);

        if (!await IsMemberAsync(householdId, request.AssigneeId))
            return new("That person isn't in your household.", false, new(), ErrorCode.InvalidInput);

        var task = result.Data!;
        task.Title      = request.Title.Trim();
        task.AssigneeId = request.AssigneeId;
        task.Due        = request.Due;
        task.Repeat     = request.Repeat;
        await taskRepository.SaveAsync();
        return new("Task updated.", true, ToResponse(task));
    }

    public async Task<Result> DeleteAsync(Guid householdId, Guid id)
    {
        var result = await taskRepository.GetByIdAsync(householdId, id);
        if (!result.Success)
            return result;

        await taskRepository.DeleteAsync(result.Data!);
        return new("Task deleted.", true);
    }

    public async Task<ValueResult<CompleteTaskResponse>> CompleteAsync(Guid householdId, Guid userId, Guid id, CompleteTaskRequest request)
    {
        if (!ClientDates.IsPlausible(request.Today, time.GetUtcNow()))
            return new(ClientDates.WrongClockMessage, false, new(), ErrorCode.InvalidInput);

        var result = await taskRepository.GetByIdAsync(householdId, id);
        if (!result.Success)
            return new(result.Message, false, new(), result.ErrorCode);

        var task = result.Data!;
        if (task.Done)
            return new("That task is already done.", false, new(), ErrorCode.Conflict);

        var completion = new TaskCompletion
        {
            Id          = Guid.NewGuid(),
            HouseholdId = householdId,
            TaskId      = task.Id,
            UserId      = userId,
            On          = request.Today,
            PreviousDue = task.Due
        };

        if (task.Repeat == TaskRepeat.None)
        {
            task.Done     = true;
            task.DoneById = userId;
            task.DoneAt   = time.GetUtcNow().UtcDateTime;
        }
        else
        {
            task.Due = Recurrence.NextDue(task.Repeat, request.Today);
        }

        taskRepository.AddCompletion(completion);
        await taskRepository.SaveAsync();
        return new("Task completed.", true, new CompleteTaskResponse { Task = ToResponse(task), CompletionId = completion.Id });
    }

    public async Task<ValueResult<TaskResponse?>> UndoCompletionAsync(Guid householdId, Guid completionId)
    {
        var result = await taskRepository.GetCompletionAsync(householdId, completionId);
        if (!result.Success)
            return new(result.Message, false, null, result.ErrorCode);

        var completion = result.Data!;
        if (completion.Task is { } task)
        {
            task.Done     = false;
            task.DoneById = null;
            task.DoneAt   = null;
            task.Due      = completion.PreviousDue;
        }

        taskRepository.RemoveCompletion(completion);
        await taskRepository.SaveAsync();
        return new("Completion undone.", true, completion.Task is null ? null : ToResponse(completion.Task));
    }

    public async Task<ValueResult<TaskResponse>> ReopenAsync(Guid householdId, Guid id)
    {
        var result = await taskRepository.GetByIdAsync(householdId, id);
        if (!result.Success)
            return new(result.Message, false, new(), result.ErrorCode);

        var task = result.Data!;
        if (task.Done)
        {
            // It wasn't done after all, so it no longer counts in the week's stats. Otherwise ticking it again counts twice.
            if (await taskRepository.GetLastCompletionAsync(householdId, id) is { } completion)
                taskRepository.RemoveCompletion(completion);

            task.Done     = false;
            task.DoneById = null;
            task.DoneAt   = null;
            await taskRepository.SaveAsync();
        }
        return new("Task reopened.", true, ToResponse(task));
    }

    private async Task<bool> IsMemberAsync(Guid householdId, Guid? userId)
    {
        if (userId is null)
            return true;
        var user = await userRepository.GetByIdAsync(userId.Value);
        return user.Success && user.Data!.HouseholdId == householdId;
    }

    private static TaskResponse ToResponse(HouseholdTask task) => new()
    {
        Id         = task.Id,
        Title      = task.Title,
        AssigneeId = task.AssigneeId,
        Due        = task.Due,
        Repeat     = task.Repeat,
        Done       = task.Done,
        DoneById   = task.DoneById,
        DoneAt     = task.DoneAt
    };
}
