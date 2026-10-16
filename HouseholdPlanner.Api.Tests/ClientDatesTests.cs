using HouseholdPlanner.Api.Services;
using static HouseholdPlanner.Api.Tests.TestData;

namespace HouseholdPlanner.Api.Tests;

public class ClientDatesTests
{
    [Theory]
    [InlineData(23, 30, 1, true)]    // 23:30 UTC: already tomorrow in UTC+1
    [InlineData(0, 30, -1, true)]    // 00:30 UTC: still yesterday in UTC−1
    [InlineData(12, 0, 2, false)]
    public void A_device_date_is_at_most_one_day_off_UTC(int hour, int minute, int offsetDays, bool plausible)
    {
        var utcNow = new DateTimeOffset(2026, 10, 15, hour, minute, 0, TimeSpan.Zero);
        Assert.Equal(plausible, ClientDates.IsPlausible(Today.AddDays(offsetDays), utcNow));
    }
}
