using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Api.Models.DTOs;

/// <summary>Creates an account in the household whose invite code this is.</summary>
public class RegisterRequest
{
    [Required] public string InviteCode { get; set; } = string.Empty;
    [Required, StringLength(50, MinimumLength = 2)] public string Username { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
}
