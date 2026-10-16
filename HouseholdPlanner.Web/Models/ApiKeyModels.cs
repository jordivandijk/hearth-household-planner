namespace HouseholdPlanner.Web.Models;

public class ApiKey
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>The start of the key, like "hearth_Xk3f".</summary>
    public string Prefix { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public class CreatedApiKey
{
    public ApiKey ApiKey { get; set; } = new();
    /// <summary>The key itself. The API sends it only this once.</summary>
    public string Key { get; set; } = string.Empty;
}

public class CreateApiKeyRequest
{
    public string Name { get; set; } = string.Empty;
}
