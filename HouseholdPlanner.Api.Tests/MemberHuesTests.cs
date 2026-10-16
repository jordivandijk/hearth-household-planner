using HouseholdPlanner.Api.Services;

namespace HouseholdPlanner.Api.Tests;

public class MemberHuesTests
{
    [Fact]
    public void Skips_hues_already_taken()
        => Assert.Equal(150, MemberHues.Next([35, 250]));

    [Fact]
    public void Still_picks_one_when_all_are_taken()
        => Assert.Contains(MemberHues.Next([35, 250, 150, 300, 200, 90, 330, 15]), new[] { 35, 250, 150, 300, 200, 90, 330, 15 });
}
