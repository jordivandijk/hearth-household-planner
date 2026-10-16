namespace HouseholdPlanner.Api.Models.Entities;

public class User : IEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>
    /// Changes with the password. Sign-in tokens carry the stamp they were made with and stop working when it
    /// changes (see Program.cs), which signs the account out on every other device. API keys don't use it.
    /// </summary>
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();
    public UserRole Role { get; set; } = UserRole.Member;
    /// <summary>Hue (0–360) of the member's avatar colour.</summary>
    public int Hue { get; set; }
    /// <summary>The language this person picked, as a two-letter code. Null until they pick one: the app then follows the device.</summary>
    public string? Language { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;
}
