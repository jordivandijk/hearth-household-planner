namespace HouseholdPlanner.Api.Models.Entities;

/// <summary>
/// Something on the shared list. Clearing the cart doesn't delete bought items: they get a
/// <see cref="ClearedAt"/>, which feeds "bought recently" and lets the clear be undone.
/// </summary>
public class GroceryItem : IEntity
{
    public const int MaxQuantity = 999;

    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public GroceryCategory Category { get; set; } = GroceryCategory.Other;
    public bool InCart { get; set; }
    public DateTime? ClearedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Household Household { get; set; } = null!;
}
