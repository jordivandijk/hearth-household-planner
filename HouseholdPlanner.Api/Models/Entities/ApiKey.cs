namespace HouseholdPlanner.Api.Models.Entities;

/// <summary>
/// A key for automations like Home Assistant or n8n. It signs in as the admin who made it. Only a hash of the key
/// is stored: the key itself is shown once, when it's made.
/// </summary>
public class ApiKey : IEntity
{
    public Guid Id { get; set; }
    /// <summary>What it's for, like "Home Assistant".</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>The start of the key, like "hearth_Xk3f", so it can be recognised in a config file.</summary>
    public string Prefix { get; set; } = string.Empty;
    /// <summary>SHA-256 of the key, in hex.</summary>
    public string Hash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    /// <summary>Roughly when it was last used, to the minute. Null until it is.</summary>
    public DateTime? LastUsedAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
