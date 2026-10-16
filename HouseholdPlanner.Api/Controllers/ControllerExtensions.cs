using System.Security.Claims;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Services.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HouseholdPlanner.Api.Controllers;

public static class ControllerExtensions
{
    public static Guid UserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>The signed-in user's household. Checked against the database on every request, see Program.cs.</summary>
    public static Guid HouseholdId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(TokenService.HouseholdClaim)!);

    /// <summary>
    /// The response for a failed result, with its message as the body, in the language of the request's
    /// Accept-Language header (see Resources/ErrorMessages.*.resx).
    /// </summary>
    public static IActionResult Error(this ControllerBase controller, Result result)
    {
        var message = result.Message is null
            ? null
            : controller.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<ErrorMessages>>()[result.Message].Value;

        return result.ErrorCode switch
        {
            ErrorCode.NotFound        => controller.NotFound(message),
            ErrorCode.InvalidInput    => controller.BadRequest(message),
            ErrorCode.DuplicateEntry  => controller.Conflict(message),
            ErrorCode.Conflict        => controller.Conflict(message),
            ErrorCode.Unauthorized    => controller.Unauthorized(message),
            ErrorCode.TooManyRequests => controller.StatusCode(429, message),
            _                         => controller.StatusCode(500, message)
        };
    }
}
