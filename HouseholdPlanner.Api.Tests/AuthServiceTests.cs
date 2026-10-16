using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Services;
using HouseholdPlanner.Api.Services.Implementations;
using static HouseholdPlanner.Api.Tests.TestData;

namespace HouseholdPlanner.Api.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeHouseholdRepository _households = new();
    private readonly SignInLimiter _limiter = new();
    private readonly AuthService _service;
    private readonly Household _home = new() { Id = HouseholdId, Name = "Home", InviteCode = "KQTM-4821" };

    public AuthServiceTests()
    {
        _households.Households.Add(_home);
        _service = new AuthService(_users, _households, new FakeTokenService(), _limiter);
    }

    public void Dispose() => _limiter.Dispose();

    private static RegisterRequest Register(string username, string code = "KQTM-4821")
        => new() { InviteCode = code, Username = username, Password = "secret1" };

    private static LoginRequest Login(string username, string password = "secret1")
        => new() { Username = username, Password = password };

    /// <summary>An account with password "secret1", hashed cheaply so tests can try it many times.</summary>
    private User Account(string name)
    {
        var user = Member(name);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret1", workFactor: 4);
        _users.Users.Add(user);
        return user;
    }

    [Fact]
    public async Task Registering_with_the_invite_code_joins_that_household_as_a_member()
    {
        var result = await _service.RegisterAsync(Register(" June ", " kqtm-4821 "));

        Assert.True(result.Success);
        Assert.Equal("token-for-June", result.Data.Token);
        var june = Assert.Single(_users.Users);
        Assert.Equal(("June", HouseholdId, UserRole.Member), (june.Username, june.HouseholdId, june.Role));
        Assert.True(BCrypt.Net.BCrypt.Verify("secret1", june.PasswordHash));
    }

    [Fact]
    public async Task An_unknown_invite_code_is_rejected()
    {
        var result = await _service.RegisterAsync(Register("June", "ABCD-0000"));

        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
        Assert.Empty(_users.Users);
    }

    [Fact]
    public async Task A_username_that_differs_only_in_case_is_taken()
    {
        _users.Users.Add(Member("june"));

        var result = await _service.RegisterAsync(Register("June"));

        Assert.Equal(ErrorCode.DuplicateEntry, result.ErrorCode);
    }

    [Fact]
    public async Task A_name_that_is_short_once_trimmed_is_rejected()
    {
        var result = await _service.RegisterAsync(Register("  J  "));

        Assert.Equal(ErrorCode.InvalidInput, result.ErrorCode);
        Assert.Empty(_users.Users);
    }

    [Fact]
    public async Task New_members_get_a_colour_nobody_in_the_household_has()
    {
        _users.Users.Add(Member("Sam", hue: 35));
        _users.Users.Add(Member("Theirs", OtherHouseholdId, hue: 250));

        await _service.RegisterAsync(Register("June"));

        Assert.Equal(250, _users.Users.Single(u => u.Username == "June").Hue);
    }

    [Fact]
    public async Task Signing_in_ignores_the_case_of_the_username_but_not_of_the_password()
    {
        await _service.RegisterAsync(Register("June"));

        var right = await _service.LoginAsync(new LoginRequest { Username = "june ", Password = "secret1" });
        var wrong = await _service.LoginAsync(new LoginRequest { Username = "June", Password = "SECRET1" });
        var unknown = await _service.LoginAsync(new LoginRequest { Username = "Nobody", Password = "secret1" });

        Assert.True(right.Success);
        Assert.Equal(ErrorCode.Unauthorized, wrong.ErrorCode);
        Assert.Equal(ErrorCode.Unauthorized, unknown.ErrorCode);
        Assert.Equal(wrong.Message, unknown.Message);
    }

    [Fact]
    public async Task After_too_many_tries_a_username_is_refused_even_with_the_right_password()
    {
        Account("June");
        // However it's typed: the same account.
        string[] spellings = ["June", " june", "JUNE "];

        for (var i = 0; i < SignInLimiter.SignInsPerUsername; i++)
            Assert.Equal(ErrorCode.Unauthorized, (await _service.LoginAsync(Login(spellings[i % 3], "guess" + i))).ErrorCode);
        var result = await _service.LoginAsync(Login("June"));

        Assert.Equal(ErrorCode.TooManyRequests, result.ErrorCode);
        Assert.Empty(result.Data.Token);
    }

    [Fact]
    public async Task Trying_one_username_too_often_leaves_the_others_alone()
    {
        Account("June");
        Account("Sam");

        for (var i = 0; i <= SignInLimiter.SignInsPerUsername; i++)
            await _service.LoginAsync(Login("June", "guess" + i));

        Assert.True((await _service.LoginAsync(Login("Sam"))).Success);
    }

    [Fact]
    public async Task All_sign_ins_together_are_limited_too()
    {
        // A few guesses each on many usernames.
        for (var i = 0; i < SignInLimiter.SignInsPerMinute; i++)
            Assert.Equal(ErrorCode.Unauthorized, (await _service.LoginAsync(Login("user" + i))).ErrorCode);

        Assert.Equal(ErrorCode.TooManyRequests, (await _service.LoginAsync(Login("one-more"))).ErrorCode);
    }

    [Fact]
    public async Task Guessing_invite_codes_is_limited()
    {
        for (var i = 0; i < SignInLimiter.SignUpsInARow; i++)
            Assert.Equal(ErrorCode.NotFound, (await _service.RegisterAsync(Register("June", $"ABCD-{i:D4}"))).ErrorCode);

        var result = await _service.RegisterAsync(Register("June"));

        Assert.Equal(ErrorCode.TooManyRequests, result.ErrorCode);
        Assert.Empty(_users.Users);
    }
}
