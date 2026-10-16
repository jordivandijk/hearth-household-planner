using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groceries")]
public class GroceryController(IGroceryService groceryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var result = await groceryService.GetListAsync(User.HouseholdId());
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Add(SaveGroceryRequest request)
    {
        var result = await groceryService.AddAsync(User.HouseholdId(), request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SaveGroceryRequest request)
    {
        var result = await groceryService.UpdateAsync(User.HouseholdId(), id, request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await groceryService.DeleteAsync(User.HouseholdId(), id);
        return result.Success ? NoContent() : this.Error(result);
    }

    [HttpPost("clear-cart")]
    public async Task<IActionResult> ClearCart()
    {
        var result = await groceryService.ClearCartAsync(User.HouseholdId());
        return Ok(result.Data);
    }

    [HttpPost("restore")]
    public async Task<IActionResult> Restore(GroceryIdsRequest request)
    {
        var result = await groceryService.RestoreAsync(User.HouseholdId(), request);
        return result.Success ? NoContent() : this.Error(result);
    }
}
