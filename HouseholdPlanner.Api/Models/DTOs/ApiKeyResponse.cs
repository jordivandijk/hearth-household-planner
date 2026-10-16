namespace HouseholdPlanner.Api.Models.DTOs;

/// <summary>A key as listed: never the key itself, which only <see cref="CreatedApiKeyResponse"/> has.</summary>
public class ApiKeyResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>The start of the key, like "hearth_Xk3f".</summary>
    public string Prefix { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public class CreatedApiKeyResponse
{
    public ApiKeyResponse ApiKey { get; set; } = new();
    /// <summary>The key. This is the only time it's sent: only its hash is stored.</summary>
    public string Key { get; set; } = string.Empty;
}
