using System.Text.Json.Serialization;

namespace HouseholdPlanner.Web.Models;

public class Household
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter<DayOfWeek>))]
    public DayOfWeek WeekStartsOn { get; set; } = DayOfWeek.Monday;
    public List<Member> Members { get; set; } = [];
}

public class Member
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Hue { get; set; }
    public string Role { get; set; } = string.Empty;
    public int DoneThisWeek { get; set; }
    public int Open { get; set; }

    public string Initial => Username.Length == 0 ? "?" : Username[..1].ToUpperInvariant();
    public bool IsAdmin => Role == "Admin";
}

public class InviteCodeResponse
{
    public string InviteCode { get; set; } = string.Empty;
}

public class SetWeekStartRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter<DayOfWeek>))]
    public DayOfWeek WeekStartsOn { get; set; }
}
