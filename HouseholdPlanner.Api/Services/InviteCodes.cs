using System.Security.Cryptography;

namespace HouseholdPlanner.Api.Services;

/// <summary>Short codes like "KQ7M-4821" that people type in when they sign up, to join a household.</summary>
public static class InviteCodes
{
    // No 0/O or 1/I/L, which are easy to mix up when reading a code out.
    private const string Letters = "ABCDEFGHJKMNPQRSTUVWXYZ";

    public static string Generate()
    {
        var letters = new string(Enumerable.Range(0, 4).Select(_ => Letters[RandomNumberGenerator.GetInt32(Letters.Length)]).ToArray());
        return $"{letters}-{RandomNumberGenerator.GetInt32(10000):D4}";
    }

    /// <summary>The code as stored: upper case without surrounding spaces, so "kq7m-4821 " still matches.</summary>
    public static string Normalize(string code) => code.Trim().ToUpperInvariant();
}
