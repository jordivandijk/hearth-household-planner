using System.Security.Cryptography;
using System.Text;

namespace HouseholdPlanner.Api.Services;

/// <summary>Keys like "hearth_Xk3f…" that automations send as "Authorization: Bearer hearth_Xk3f…".</summary>
public static class ApiKeySecrets
{
    /// <summary>Tells a key apart from a sign-in token, which never starts with this.</summary>
    public const string Start = "hearth_";

    /// <summary>How much of the key is kept to recognise it by: the start and 4 random characters.</summary>
    public const int PrefixLength = 11;

    /// <summary>A new key: the start, then 32 random bytes in URL-safe base64 (43 characters).</summary>
    public static string Generate() => Start + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static bool IsKey(string token) => token.StartsWith(Start, StringComparison.Ordinal);

    /// <summary>
    /// What's stored instead of the key. A plain hash is enough: the key is random, so unlike a password there's
    /// nothing to guess, and a fast hash keeps checking a key on every request cheap.
    /// </summary>
    public static string Hash(string key) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)));

    public static string Prefix(string key) => key[..PrefixLength];
}
