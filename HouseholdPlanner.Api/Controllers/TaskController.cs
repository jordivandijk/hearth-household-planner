using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TaskController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await taskService.GetAllAsync(User.HouseholdId());
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaveTaskRequest request)
    {
        var result = await taskService.CreateAsync(User.HouseholdId(), request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SaveTaskRequest request)
    {
        var result = await taskService.UpdateAsync(User.HouseholdId(), id, request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await taskService.DeleteAsync(User.HouseholdId(), id);
        return result.Success ? NoContent() : this.Error(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CompleteTaskRequest request)
    {
        var result = await taskService.CompleteAsync(User.HouseholdId(), User.UserId(), id, request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id)
    {
        var result = await taskService.ReopenAsync(User.HouseholdId(), id);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    /// <summary>Undoes a completion. Returns the task as it is again, or 204 when it was deleted meanwhile.</summary>
    [HttpDelete("completions/{completionId:guid}")]
    public async Task<IActionResult> UndoCompletion(Guid completionId)
    {
        var result = await taskService.UndoCompletionAsync(User.HouseholdId(), completionId);
        if (!result.Success)
            return this.Error(result);
        return result.Data is null ? NoContent() : Ok(result.Data);
    }
}
