using System.Threading.RateLimiting;

namespace HouseholdPlanner.Api.Services;

/// <summary>
/// Limits how often someone can try to sign in or sign up, so a password or an invite code can't be found by trying
/// many. Sign-ins are counted per username rather than per address: behind a reverse proxy every request comes from
/// the proxy, and X-Forwarded-For can be made up when the API is reachable directly. A limit on all sign-ins together
/// slows down trying a few passwords on many usernames.
/// </summary>
/// <remarks>
/// ASP.NET Core's rate limiting middleware can't do this: it picks the limit before the request body is read, and
/// the username is in the body. So the services ask here, with the same limiters the middleware uses.
/// </remarks>
public sealed class SignInLimiter : IDisposable
{
    /// <summary>Sign-ins per username in a row; after that, one more every <see cref="UsernameRefill"/>.</summary>
    public const int SignInsPerUsername = 10;
    public static readonly TimeSpan UsernameRefill = TimeSpan.FromMinutes(6);

    /// <summary>Sign-ins a minute for all usernames together.</summary>
    public const int SignInsPerMinute = 30;

    /// <summary>
    /// New accounts in a row; after that, one more every <see cref="SignUpRefill"/>. The username is up to whoever
    /// signs up, so only the total counts. Invite codes have about 31 bits, too few to go without a limit.
    /// </summary>
    public const int SignUpsInARow = 10;
    public static readonly TimeSpan SignUpRefill = TimeSpan.FromMinutes(6);

    private readonly PartitionedRateLimiter<string> _perUsername = PartitionedRateLimiter.Create<string, string>(username =>
        RateLimitPartition.GetTokenBucketLimiter(username, _ => Bucket(SignInsPerUsername, 1, UsernameRefill)));

    private readonly RateLimiter _allSignIns = new TokenBucketRateLimiter(Bucket(SignInsPerMinute, SignInsPerMinute, TimeSpan.FromMinutes(1)));

    private readonly RateLimiter _signUps = new TokenBucketRateLimiter(Bucket(SignUpsInARow, 1, SignUpRefill));

    /// <summary>
    /// Counts a sign-in attempt, before the password is checked. False when there have been too many. Usernames are
    /// matched like at sign-in, ignoring case and surrounding spaces.
    /// </summary>
    public bool TrySignIn(string username)
    {
        // Per username first, so trying one account over and over doesn't use up everyone's sign-ins.
        using var forUsername = _perUsername.AttemptAcquire(username.Trim().ToLowerInvariant());
        if (!forUsername.IsAcquired)
            return false;

        using var forAll = _allSignIns.AttemptAcquire();
        return forAll.IsAcquired;
    }

    /// <summary>Counts an attempt to sign up. False when there have been too many.</summary>
    public bool TrySignUp()
    {
        using var lease = _signUps.AttemptAcquire();
        return lease.IsAcquired;
    }

    private static TokenBucketRateLimiterOptions Bucket(int limit, int refill, TimeSpan every) => new()
    {
        TokenLimit          = limit,
        TokensPerPeriod     = refill,
        ReplenishmentPeriod = every,
        QueueLimit          = 0
    };

    public void Dispose()
    {
        _perUsername.Dispose();
        _allSignIns.Dispose();
        _signUps.Dispose();
    }
}
