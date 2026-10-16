using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPlanner.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }

    /// <summary>Creates an account in the household of the invite code and returns a token, like a login.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return result.Success ? Ok(result.Data) : this.Error(result);
    }
}
