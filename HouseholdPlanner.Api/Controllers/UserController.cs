using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await userService.GetByIdAsync(User.UserId());
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    /// <summary>Removes someone from the admin's household and deletes their account.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Remove(Guid id)
    {
        var result = await userService.RemoveMemberAsync(User.HouseholdId(), User.UserId(), id);
        return result.Success ? NoContent() : this.Error(result);
    }

    [HttpPut("me/language")]
    public async Task<IActionResult> SetLanguage(SetLanguageRequest request)
    {
        var result = await userService.SetLanguageAsync(User.UserId(), request);
        return result.Success ? NoContent() : this.Error(result);
    }

    /// <summary>Changes the password, which signs out every other device. Returns a new token for this one.</summary>
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var result = await userService.ChangePasswordAsync(User.UserId(), request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }
}
