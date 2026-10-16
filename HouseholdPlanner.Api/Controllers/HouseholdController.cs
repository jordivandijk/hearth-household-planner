using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/household")]
public class HouseholdController(IHouseholdService householdService) : ControllerBase
{
    /// <summary>The household, its invite code, and per member what they did this week.</summary>
    /// <param name="today">Today in the user's time zone; the server's date can be a day off.</param>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly today)
    {
        var result = await householdService.GetAsync(User.HouseholdId(), today);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    /// <summary>Replaces the invite code, e.g. after removing someone, so the old code stops working.</summary>
    [HttpPost("invite-code")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RenewInviteCode()
    {
        var result = await householdService.RenewInviteCodeAsync(User.HouseholdId());
        // An object, since a bare string would go out as text/plain rather than JSON.
        return result.Success ? Ok(new { InviteCode = result.Data }) : this.Error(result);
    }

    /// <summary>Sets the day the household's weeks start on, for "This week" and "Next week" in the task list.</summary>
    [HttpPut("week-start")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> SetWeekStart(SetWeekStartRequest request)
    {
        var result = await householdService.SetWeekStartAsync(User.HouseholdId(), request);
        return result.Success ? NoContent() : this.Error(result);
    }
}
