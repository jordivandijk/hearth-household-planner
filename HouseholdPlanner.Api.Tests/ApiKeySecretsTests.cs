using System.Text.RegularExpressions;
using HouseholdPlanner.Api.Authentication;
using HouseholdPlanner.Api.Services;
using Microsoft.AspNetCore.Http;

namespace HouseholdPlanner.Api.Tests;

public class ApiKeySecretsTests
{
    [Fact]
    public void Keys_are_the_start_and_43_url_safe_characters()
        => Assert.Matches(new Regex("^hearth_[A-Za-z0-9_-]{43}$"), ApiKeySecrets.Generate());

    [Fact]
    public void The_hash_is_the_same_every_time_and_differs_per_key()
    {
        var key = ApiKeySecrets.Generate();

        Assert.Equal(ApiKeySecrets.Hash(key), ApiKeySecrets.Hash(key));
        Assert.NotEqual(ApiKeySecrets.Hash(key), ApiKeySecrets.Hash(ApiKeySecrets.Generate()));
        Assert.Equal(64, ApiKeySecrets.Hash(key).Length);
    }

    [Theory]
    [InlineData("Bearer hearth_abc", "hearth_abc")]
    [InlineData("bearer  hearth_abc ", "hearth_abc")]
    [InlineData("Bearer eyJhbGciOiJIUzI1NiJ9.e30.x", null)]   // a sign-in token
    [InlineData("Basic aGVhcnRoXzo=", null)]
    [InlineData("", null)]
    public void A_key_is_recognised_in_the_Authorization_header(string header, string? expected)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = header;

        Assert.Equal(expected, ApiKeyAuthenticationHandler.KeyFrom(context.Request));
    }
}
