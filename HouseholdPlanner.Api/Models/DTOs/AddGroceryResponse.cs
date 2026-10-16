namespace HouseholdPlanner.Api.Models.DTOs;

public class AddGroceryResponse
{
    public GroceryResponse Item { get; set; } = new();
    /// <summary>The name was already on the list, so its quantity went up instead of adding a second line.</summary>
    public bool Merged { get; set; }
    /// <summary>The quantity before a merge, to undo it.</summary>
    public int PreviousQuantity { get; set; }
}
