using System.Text.Json.Serialization;

namespace HouseholdPlanner.Web.Models;

/// <summary>Supermarket aisles, in the order the list shows them.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GroceryCategory>))]
public enum GroceryCategory
{
    Produce,
    DairyEggs,
    Bakery,
    MeatFish,
    Pantry,
    Frozen,
    Household,
    Other
}

public class GroceryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public GroceryCategory Category { get; set; } = GroceryCategory.Other;
    public bool InCart { get; set; }

    public SaveGroceryRequest ToRequest() => new() { Name = Name, Quantity = Quantity, Category = Category, InCart = InCart };
}

public class GroceryList
{
    public List<GroceryItem> Items { get; set; } = [];
    public List<string> Recents { get; set; } = [];
}

public class SaveGroceryRequest
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public GroceryCategory Category { get; set; } = GroceryCategory.Other;
    public bool InCart { get; set; }
}

public class AddGroceryResponse
{
    public GroceryItem Item { get; set; } = new();
    public bool Merged { get; set; }
    public int PreviousQuantity { get; set; }
}

public class GroceryIdsRequest
{
    public List<Guid> Ids { get; set; } = [];
}
