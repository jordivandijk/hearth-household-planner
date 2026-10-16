using System.IdentityModel.Tokens.Jwt;
using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Services.Implementations;
using Microsoft.Extensions.Configuration;
using static HouseholdPlanner.Api.Tests.TestData;

namespace HouseholdPlanner.Api.Tests;

public class UserServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly UserService _service;
    private readonly User _admin = Member("Sam");
    private readonly User _june = Member("June");

    public UserServiceTests()
    {
        _admin.Role = UserRole.Admin;
        _users.Users.AddRange([_admin, _june]);
        _service = new UserService(_users, new FakeTokenService());
    }

    [Fact]
    public async Task An_admin_can_remove_someone_from_their_household()
    {
        var result = await _service.RemoveMemberAsync(HouseholdId, _admin.Id, _june.Id);

        Assert.True(result.Success);
        Assert.DoesNotContain(_june, _users.Users);
    }

    [Fact]
    public async Task An_admin_cannot_remove_themselves()
    {
        var result = await _service.RemoveMemberAsync(HouseholdId, _admin.Id, _admin.Id);

        Assert.Equal(ErrorCode.InvalidInput, result.ErrorCode);
        Assert.Contains(_admin, _users.Users);
    }

    [Fact]
    public async Task Someone_from_another_household_is_not_found()
    {
        var outsider = Member("Rio", OtherHouseholdId);
        _users.Users.Add(outsider);

        var result = await _service.RemoveMemberAsync(HouseholdId, _admin.Id, outsider.Id);

        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
        Assert.Contains(outsider, _users.Users);
    }

    [Fact]
    public async Task An_unknown_account_is_not_found()
        => Assert.Equal(ErrorCode.NotFound, (await _service.RemoveMemberAsync(HouseholdId, _admin.Id, Guid.NewGuid())).ErrorCode);

    [Theory]
    [InlineData("nl")]
    [InlineData("en")]
    public async Task A_supported_language_is_saved_on_the_account(string language)
    {
        var result = await _service.SetLanguageAsync(_june.Id, new SetLanguageRequest { Language = language });

        Assert.True(result.Success);
        Assert.Equal(language, _june.Language);
        Assert.Equal(language, (await _service.GetByIdAsync(_june.Id)).Data.Language);
    }

    [Theory]
    [InlineData("de")]
    [InlineData("NL")]
    [InlineData("")]
    public async Task Other_languages_are_refused(string language)
    {
        var result = await _service.SetLanguageAsync(_june.Id, new SetLanguageRequest { Language = language });

        Assert.Equal(ErrorCode.InvalidInput, result.ErrorCode);
        Assert.Null(_june.Language);
    }

    [Fact]
    public async Task A_new_account_has_no_language_so_the_app_follows_the_device()
        => Assert.Null((await _service.GetByIdAsync(_june.Id)).Data.Language);

    [Fact]
    public async Task Changing_the_password_needs_the_current_one()
    {
        _june.PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret1");

        var wrong = await _service.ChangePasswordAsync(_june.Id, new ChangePasswordRequest { CurrentPassword = "nope", NewPassword = "secret2" });
        var right = await _service.ChangePasswordAsync(_june.Id, new ChangePasswordRequest { CurrentPassword = "secret1", NewPassword = "secret2" });

        Assert.Equal(ErrorCode.InvalidInput, wrong.ErrorCode);
        Assert.True(right.Success);
        Assert.True(BCrypt.Net.BCrypt.Verify("secret2", _june.PasswordHash));
    }

    [Fact]
    public async Task Changing_the_password_signs_out_other_devices_and_gives_this_one_a_new_token()
    {
        _june.PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret1", workFactor: 4);
        var before = _june.SecurityStamp;
        var tokens = new TokenService(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"]          = "a-test-key-that-is-at-least-32-bytes-long",
            ["Jwt:Issuer"]       = "test",
            ["Jwt:Audience"]     = "test",
            ["Jwt:ExpiryInDays"] = "30"
        }).Build());
        var service = new UserService(_users, tokens);

        var result = await service.ChangePasswordAsync(_june.Id, new ChangePasswordRequest { CurrentPassword = "secret1", NewPassword = "secret2" });

        // Tokens are checked against the account's stamp on every request (Program.cs), so the old ones stop working.
        Assert.NotEqual(before, _june.SecurityStamp);
        var stamp = new JwtSecurityTokenHandler().ReadJwtToken(result.Data.Token).Claims.Single(c => c.Type == TokenService.SecurityStampClaim);
        Assert.Equal(_june.SecurityStamp.ToString(), stamp.Value);
    }

    [Fact]
    public async Task A_wrong_current_password_signs_nobody_out()
    {
        _june.PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret1", workFactor: 4);
        var before = _june.SecurityStamp;

        await _service.ChangePasswordAsync(_june.Id, new ChangePasswordRequest { CurrentPassword = "nope", NewPassword = "secret2" });

        Assert.Equal(before, _june.SecurityStamp);
    }
}
