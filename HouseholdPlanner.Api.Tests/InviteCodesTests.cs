using System.Text.RegularExpressions;
using HouseholdPlanner.Api.Services;

namespace HouseholdPlanner.Api.Tests;

public class InviteCodesTests
{
    [Fact]
    public void Codes_are_four_letters_a_dash_and_four_digits()
        => Assert.Matches(new Regex("^[A-Z]{4}-[0-9]{4}$"), InviteCodes.Generate());

    [Fact]
    public void Typed_codes_ignore_case_and_spaces()
        => Assert.Equal("KQTM-4821", InviteCodes.Normalize("  kqtm-4821 "));
}
