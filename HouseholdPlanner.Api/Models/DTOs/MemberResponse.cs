using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Models.DTOs;

public class MemberResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Hue { get; set; }
    public UserRole Role { get; set; }
    /// <summary>Tasks this member did this calendar week, which starts on the household's week start.</summary>
    public int DoneThisWeek { get; set; }
    /// <summary>Open tasks assigned to this member.</summary>
    public int Open { get; set; }
}
