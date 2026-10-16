namespace HouseholdPlanner.Api.Models.DTOs;

public class GroceryListResponse
{
    public List<GroceryResponse> Items { get; set; } = [];
    /// <summary>Names of things bought recently, newest first.</summary>
    public List<string> Recents { get; set; } = [];
}
