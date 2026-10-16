using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Models.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int Hue { get; set; }
    /// <summary>Null when this person hasn't picked a language.</summary>
    public string? Language { get; set; }
    public Guid HouseholdId { get; set; }
}
