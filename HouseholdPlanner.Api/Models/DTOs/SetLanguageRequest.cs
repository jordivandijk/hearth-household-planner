using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Api.Models.DTOs;

public class SetLanguageRequest
{
    /// <summary>A two-letter code, like "en" or "nl".</summary>
    [Required] public string Language { get; set; } = string.Empty;
}
