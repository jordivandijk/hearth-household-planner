namespace HouseholdPlanner.Api.Services;

/// <summary>
/// "Today" comes from the user's device, since the server doesn't know their time zone. Time zones
/// run from UTC−12 to UTC+14, so a device's date is at most one day off the UTC date. Anything
/// further is a wrong clock or a missing value (which binds as 0001-01-01), not a time zone.
/// </summary>
public static class ClientDates
{
    public const string WrongClockMessage = "Your device's date looks wrong. Check its clock and try again.";

    public static bool IsPlausible(DateOnly today, DateTimeOffset utcNow)
        => Math.Abs(today.DayNumber - DateOnly.FromDateTime(utcNow.UtcDateTime).DayNumber) <= 1;
}
