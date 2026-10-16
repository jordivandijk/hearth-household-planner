using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Tests;

internal static class TestData
{
    /// <summary>Thursday 15 October 2026, 10:00 UTC.</summary>
    public static readonly DateTimeOffset Now = new(2026, 10, 15, 10, 0, 0, TimeSpan.Zero);
    public static readonly DateOnly Today = new(2026, 10, 15);

    public static readonly Guid HouseholdId = Guid.NewGuid();
    public static readonly Guid OtherHouseholdId = Guid.NewGuid();

    public static User Member(string name, Guid? householdId = null, int hue = 35) => new()
    {
        Id          = Guid.NewGuid(),
        Username    = name,
        Hue         = hue,
        HouseholdId = householdId ?? HouseholdId
    };

    public static HouseholdTask Chore(string title, DateOnly? due = null, TaskRepeat repeat = TaskRepeat.None,
        Guid? assigneeId = null, Guid? householdId = null) => new()
    {
        Id          = Guid.NewGuid(),
        HouseholdId = householdId ?? HouseholdId,
        Title       = title,
        Due         = due,
        Repeat      = repeat,
        AssigneeId  = assigneeId
    };

    public static GroceryItem Grocery(string name, int quantity = 1, bool inCart = false, DateTime? clearedAt = null,
        Guid? householdId = null) => new()
    {
        Id          = Guid.NewGuid(),
        HouseholdId = householdId ?? HouseholdId,
        Name        = name,
        Quantity    = quantity,
        InCart      = inCart,
        ClearedAt   = clearedAt
    };
}
