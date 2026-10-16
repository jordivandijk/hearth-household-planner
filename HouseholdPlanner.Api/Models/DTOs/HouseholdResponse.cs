namespace HouseholdPlanner.Api.Models.DTOs;

public class HouseholdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public DayOfWeek WeekStartsOn { get; set; }
    public List<MemberResponse> Members { get; set; } = [];
}
