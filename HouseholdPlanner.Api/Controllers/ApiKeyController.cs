using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPlanner.Api.Controllers;

/// <summary>
/// The admin's keys for automations. Only for a signed-in admin, not for an API key: a leaked key can't be used to
/// make more keys, or to hide by revoking the others.
/// </summary>
[ApiController]
[Authorize(Policy = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/api-keys")]
public class ApiKeyController(IApiKeyService apiKeyService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await apiKeyService.GetAllAsync(User.UserId()));

    [HttpPost]
    public async Task<IActionResult> Create(CreateApiKeyRequest request)
    {
        var result = await apiKeyService.CreateAsync(User.UserId(), request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var result = await apiKeyService.RevokeAsync(User.UserId(), id);
        return result.Success ? NoContent() : this.Error(result);
    }
}
