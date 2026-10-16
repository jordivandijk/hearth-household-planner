using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Api.Models.DTOs;

public class CreateApiKeyRequest
{
    /// <summary>What it's for, like "Home Assistant".</summary>
    [Required, StringLength(50, MinimumLength = 1)] public string Name { get; set; } = string.Empty;
}
