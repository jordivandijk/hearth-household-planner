using HouseholdPlanner.Api.Models.Entities;

namespace HouseholdPlanner.Api.Models.DTOs;

public class GroceryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public GroceryCategory Category { get; set; }
    public bool InCart { get; set; }
}
